using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Security;

namespace Shivakala.Infrastructure.Services;

public sealed class PortalUserService(
    ShivakalaDbContext db,
    Shivakala.Core.Interfaces.ITeacherRepository teacherRepo,
    ILogger<PortalUserService> logger) : IPortalUserService
{
    public async Task<AppUser?> ValidateCredentialsAsync(string username, string password, string role, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        AppUser? user = null;
        if (role == "Teacher")
        {
            user = await FindTeacherUserAsync(username, ct);
        }
        else if (role == "Student")
        {
            user = await FindStudentUserAsync(username, ct);
        }
        else if (role == "MpscStudent")
        {
            user = await FindMpscUserAsync(username, ct);
        }
        else
        {
            user = await db.AppUsers.FirstOrDefaultAsync(
                u => u.Username == username.Trim() && u.Role == role && u.IsActive, ct);
        }

        if (user is null || !VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public Task<AppUser?> FindByIdAsync(int userId, CancellationToken ct = default)
        => db.AppUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);

    public async Task<AppUser?> FindTeacherUserAsync(string login, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login)) return null;

        var normalized = login.Trim().ToLowerInvariant();
        var mobile = NormalizeMobile(login);

        var user = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Role == "Teacher" && u.IsActive && u.Username.ToLower() == normalized, ct);
        if (user is not null) return user;

        if (mobile.Length == 10)
        {
            user = await db.AppUsers.FirstOrDefaultAsync(
                u => u.Role == "Teacher" && u.IsActive && u.Mobile == mobile, ct);
            if (user is not null) return user;
        }

        var teachers = (await teacherRepo.GetAllAsync(ct)).Where(t => t.IsActive).ToList();
        var teacher = teachers.FirstOrDefault(t =>
            (!string.IsNullOrWhiteSpace(t.EmployeeCode)
                && string.Equals(t.EmployeeCode.Trim(), login.Trim(), StringComparison.OrdinalIgnoreCase))
            || (mobile.Length == 10 && NormalizeMobile(t.Mobile) == mobile)
            || string.Equals(t.Mobile.Trim(), login.Trim(), StringComparison.OrdinalIgnoreCase));

        if (teacher is null) return null;

        return await db.AppUsers.FirstOrDefaultAsync(
            u => u.TeacherId == teacher.Id && u.Role == "Teacher" && u.IsActive, ct);
    }

    public async Task<AppUser?> FindStudentUserAsync(string login, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login)) return null;

        var normalized = login.Trim().ToLowerInvariant();
        var mobile = NormalizeMobile(login);

        var user = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Role == "Student" && u.IsActive && u.Username.ToLower() == normalized, ct);
        if (user is not null) return user;

        if (mobile.Length == 10)
        {
            user = await db.AppUsers.FirstOrDefaultAsync(
                u => u.Role == "Student" && u.IsActive && u.Mobile == mobile, ct);
            if (user is not null) return user;
        }

        var student = await db.Students.FirstOrDefaultAsync(s =>
            (s.RollNumber != null && s.RollNumber.ToLower() == normalized) ||
            (s.AdmissionNumber != null && s.AdmissionNumber.ToLower() == normalized) ||
            (mobile.Length == 10 && s.Mobile != null && s.Mobile.EndsWith(mobile)), ct);

        if (student is null) return null;

        return await db.AppUsers.FirstOrDefaultAsync(
            u => u.StudentId == student.Id && u.Role == "Student" && u.IsActive, ct);
    }

    public async Task<AppUser?> FindMpscUserAsync(string login, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login)) return null;

        var normalized = login.Trim().ToLowerInvariant();
        var mobile = NormalizeMobile(login);

        // Match AppUser directly by username
        var user = await db.AppUsers.Include(u => u.MpscStudent).FirstOrDefaultAsync(
            u => u.Role == "MpscStudent" && u.IsActive && u.Username.ToLower() == normalized, ct);
        if (user is not null) return user;

        // Match AppUser directly by email
        user = await db.AppUsers.Include(u => u.MpscStudent).FirstOrDefaultAsync(
            u => u.Role == "MpscStudent" && u.IsActive && u.Email.ToLower() == normalized, ct);
        if (user is not null) return user;

        // Match AppUser by mobile
        if (mobile.Length == 10)
        {
            user = await db.AppUsers.Include(u => u.MpscStudent).FirstOrDefaultAsync(
                u => u.Role == "MpscStudent" && u.IsActive && u.Mobile == mobile, ct);
            if (user is not null) return user;
        }

        // Match candidate record by RegistrationNumber, Email, or Mobile
        var candidate = await db.MpscStudents.FirstOrDefaultAsync(m =>
            m.RegistrationNumber.ToLower() == normalized ||
            m.Email.ToLower() == normalized ||
            (mobile.Length == 10 && m.Mobile.EndsWith(mobile)), ct);

        if (candidate is null) return null;

        return await db.AppUsers.Include(u => u.MpscStudent).FirstOrDefaultAsync(
            u => u.MpscStudentId == candidate.Id && u.Role == "MpscStudent" && u.IsActive, ct);
    }

    public async Task<AppUser> EnsureTeacherAccountAsync(int teacherId, string? username = null, string? password = null, CancellationToken ct = default)
    {
        var teacher = await teacherRepo.GetByIdAsync(teacherId, ct)
            ?? throw new InvalidOperationException($"Teacher #{teacherId} not found.");

        var existing = await db.AppUsers.FirstOrDefaultAsync(
            u => u.TeacherId == teacherId && u.Role == "Teacher", ct);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(username))
                existing.Username = SanitizeUsername(username);
            if (!string.IsNullOrWhiteSpace(password))
                existing.PasswordHash = HashPassword(password);
            else if (existing.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
                existing.PasswordHash = HashPassword(DefaultPasswordFromMobile(teacher.Mobile));
            existing.FullName = teacher.FullName;
            existing.Mobile = teacher.Mobile;
            existing.Email = teacher.Email ?? PortalEmail(existing.Username);
            existing.IsActive = teacher.IsActive;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var finalUsername = !string.IsNullOrWhiteSpace(username)
            ? SanitizeUsername(username)
            : await GenerateUniqueTeacherUsernameAsync(teacher, ct);

        var finalPassword = !string.IsNullOrWhiteSpace(password)
            ? password
            : DefaultPasswordFromMobile(teacher.Mobile);

        var user = new AppUser
        {
            Username = finalUsername,
            Email = teacher.Email ?? PortalEmail(finalUsername),
            PasswordHash = HashPassword(finalPassword),
            Role = "Teacher",
            FullName = teacher.FullName,
            Mobile = teacher.Mobile,
            TeacherId = teacherId,
            IsActive = teacher.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Teacher portal account created: {Username} (Teacher #{Id})", user.Username, teacherId);
        return user;
    }

    public async Task<AppUser?> EnsureParentAccountForStudentAsync(int studentId, string? password = null, CancellationToken ct = default)
    {
        var student = await db.Students.FindAsync([studentId], ct);
        if (student is null) return null;

        var mobile = NormalizeMobile(student.ParentMobile ?? student.Mobile);
        if (mobile.Length != 10) return null;

        var existing = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Username == mobile && u.Role == "Parent", ct);

        if (existing is not null)
        {
            existing.StudentId = studentId;
            existing.FullName = student.ParentName ?? student.FullName;
            existing.Mobile = mobile;
            if (!string.IsNullOrWhiteSpace(password))
                existing.PasswordHash = HashPassword(password);
            else if (existing.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
                existing.PasswordHash = HashPassword(DefaultPasswordFromMobile(mobile));
            existing.IsActive = true;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var finalPassword = !string.IsNullOrWhiteSpace(password)
            ? password
            : DefaultPasswordFromMobile(mobile);

        var user = new AppUser
        {
            Username = mobile,
            Email = PortalEmail(mobile),
            PasswordHash = HashPassword(finalPassword),
            Role = "Parent",
            FullName = student.ParentName ?? $"Parent of {student.FullName}",
            Mobile = mobile,
            StudentId = studentId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Parent portal account created for student #{Id} (username: {Username})", studentId, mobile);
        return user;
    }

    public async Task<AppUser?> EnsureStudentAccountAsync(int studentId, string? username = null, string? password = null, CancellationToken ct = default)
    {
        var student = await db.Students.FindAsync([studentId], ct);
        if (student is null) return null;

        var existing = await db.AppUsers.FirstOrDefaultAsync(
            u => u.StudentId == studentId && u.Role == "Student", ct);

        var mobile = NormalizeMobile(student.Mobile);
        var baseUsername = !string.IsNullOrWhiteSpace(username)
            ? SanitizeUsername(username)
            : !string.IsNullOrWhiteSpace(student.RollNumber)
                ? SanitizeUsername(student.RollNumber)
                : mobile.Length == 10 ? mobile : $"student{studentId}";

        var finalUsername = baseUsername;
        if (existing is null && await db.AppUsers.AnyAsync(u => u.Username == finalUsername, ct))
        {
            finalUsername = $"{baseUsername}_{studentId}";
        }

        var finalPassword = !string.IsNullOrWhiteSpace(password)
            ? password
            : DefaultPasswordFromMobile(mobile);

        var finalEmail = (!string.IsNullOrWhiteSpace(student.Email) && !await db.AppUsers.AnyAsync(u => u.Email == student.Email && (existing == null || u.Id != existing.Id), ct))
            ? student.Email
            : PortalEmail(finalUsername);

        if (existing is not null)
        {
            existing.Username = finalUsername;
            existing.FullName = student.FullName;
            existing.Mobile = student.Mobile;
            existing.Email = finalEmail;
            if (!string.IsNullOrWhiteSpace(password))
                existing.PasswordHash = HashPassword(password);
            existing.IsActive = true;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var user = new AppUser
        {
            Username = finalUsername,
            Email = finalEmail,
            PasswordHash = HashPassword(finalPassword),
            Role = "Student",
            FullName = student.FullName,
            Mobile = student.Mobile,
            StudentId = studentId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Student portal account created for student #{Id} (username: {Username})", studentId, user.Username);
        return user;
    }

    public async Task<AppUser> EnsureAdminAccountAsync(string username, string password, CancellationToken ct = default)
    {
        var finalUsername = SanitizeUsername(username);
        var existing = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Role == "Admin" && u.Username == finalUsername, ct);

        if (existing is not null)
        {
            existing.IsActive = true;
            if (string.IsNullOrWhiteSpace(existing.FullName))
                existing.FullName = "Administrator";
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var anyAdminExists = await db.AppUsers.AnyAsync(u => u.Role == "Admin", ct);
        if (anyAdminExists)
        {
            var firstAdmin = await db.AppUsers.FirstAsync(u => u.Role == "Admin", ct);
            return firstAdmin;
        }

        var admin = new AppUser
        {
            Username = finalUsername,
            Email = PortalEmail(finalUsername),
            PasswordHash = HashPassword(password),
            Role = "Admin",
            FullName = "Administrator",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(admin);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Admin portal account created for username: {Username}", admin.Username);
        return admin;
    }

    public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user is null)
            return (false, "User account not found.");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect.");

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        await db.SaveChangesAsync(ct);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> SetPasswordAsync(int userId, string newPassword, CancellationToken ct = default)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return (false, "User account not found.");

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        await db.SaveChangesAsync(ct);
        return (true, string.Empty);
    }

    public async Task SyncMissingPortalAccountsAsync(CancellationToken ct = default)
    {
        var teachers = (await teacherRepo.GetAllAsync(ct)).Where(t => t.IsActive).ToList();
        foreach (var teacher in teachers)
        {
            if (await db.AppUsers.AnyAsync(u => u.TeacherId == teacher.Id && u.Role == "Teacher", ct))
                continue;
            await EnsureTeacherAccountAsync(teacher.Id, ct: ct);
        }

        var students = await db.Students
            .Where(s => s.Status == "Admitted")
            .ToListAsync(ct);

        foreach (var student in students)
        {
            // Sync Parent account
            var mobile = NormalizeMobile(student.ParentMobile ?? student.Mobile);
            if (mobile.Length == 10 && !await db.AppUsers.AnyAsync(u => u.Username == mobile && u.Role == "Parent", ct))
            {
                await EnsureParentAccountForStudentAsync(student.Id, ct: ct);
            }

            // Sync Student account
            if (!await db.AppUsers.AnyAsync(u => u.StudentId == student.Id && u.Role == "Student", ct))
            {
                await EnsureStudentAccountAsync(student.Id, ct: ct);
            }
        }
    }

    private async Task<string> GenerateUniqueTeacherUsernameAsync(Teacher teacher, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(teacher.EmployeeCode))
        {
            var fromCode = SanitizeUsername(teacher.EmployeeCode);
            if (!await db.AppUsers.AnyAsync(u => u.Username == fromCode, ct))
                return fromCode;
        }

        var baseName = SanitizeUsername(teacher.FullName.Replace(" ", "").ToLowerInvariant());
        if (baseName.Length < 3) baseName = "teacher";
        var candidate = baseName;
        var suffix = 1;
        while (await db.AppUsers.AnyAsync(u => u.Username == candidate, ct))
            candidate = $"{baseName}{suffix++}";

        return candidate;
    }

    private static bool VerifyPassword(string password, string hash)
        => PasswordHasher.Verify(password, hash);

    private static string HashPassword(string password) => PasswordHasher.Hash(password);

    private static string DefaultPasswordFromMobile(string mobile)
    {
        var digits = NormalizeMobile(mobile);
        return digits.Length >= 4 ? digits[^4..] : "1234";
    }

    private static string NormalizeMobile(string? mobile)
    {
        var digits = new string((mobile ?? "").Where(char.IsDigit).ToArray());
        return digits.Length >= 10 ? digits[^10..] : digits;
    }

    private static string SanitizeUsername(string value)
    {
        var s = new string(value.Trim().ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(s) ? "user" : s;
    }

    private static string PortalEmail(string username) => $"{username}@portal.shivakala.local";
}
