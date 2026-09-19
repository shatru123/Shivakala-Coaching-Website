using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Security;

namespace Shivakala.Infrastructure.Services;

public sealed class MpscStudentService(
    ShivakalaDbContext db,
    IAuditService audit,
    IWebHostEnvironment env,
    ILogger<MpscStudentService> logger) : IMpscStudentService
{
    // ── Registration & Auth ──────────────────────────────────────────────────

    public async Task<(bool Success, string Message, MpscStudent? Candidate, string? GeneratedPassword)> RegisterCandidateAsync(
        MpscRegistrationViewModel model, string? ipAddress = null, CancellationToken ct = default)
    {
        var cleanMobile = NormalizeMobile(model.Mobile);
        var cleanEmail = model.Email.Trim().ToLowerInvariant();

        if (cleanMobile.Length != 10)
            return (false, "Please provide a valid 10-digit Indian mobile number.", null, null);

        // Check duplicates in MpscStudent domain
        var mobileExists = await db.MpscStudents.AnyAsync(s => s.Mobile == cleanMobile, ct);
        if (mobileExists)
            return (false, "An MPSC candidate with this mobile number is already registered.", null, null);

        var emailExists = await db.MpscStudents.AnyAsync(s => s.Email.ToLower() == cleanEmail, ct);
        if (emailExists)
            return (false, "An MPSC candidate with this email address is already registered.", null, null);

        // Check if an AppUser already exists with this email or username
        var userExists = await db.AppUsers.AnyAsync(u => u.Email.ToLower() == cleanEmail, ct);
        if (userExists)
            return (false, "An account with this email address already exists.", null, null);

        // Generate standard registration number: SK-MPSC-{yyyy}-{seq:D5}
        var year = DateTime.UtcNow.Year;
        var prefix = $"SK-MPSC-{year}-";
        var lastCandidate = await db.MpscStudents
            .Where(s => s.RegistrationNumber.StartsWith(prefix))
            .OrderByDescending(s => s.RegistrationNumber)
            .Select(s => s.RegistrationNumber)
            .FirstOrDefaultAsync(ct);

        int seq = 1;
        if (!string.IsNullOrWhiteSpace(lastCandidate) && lastCandidate.Length >= prefix.Length + 5)
        {
            var seqStr = lastCandidate.Substring(prefix.Length);
            if (int.TryParse(seqStr, out var parsedSeq))
                seq = parsedSeq + 1;
        }

        var regNumber = $"{prefix}{seq:D5}";

        // Photo upload handling if provided
        string? photoUrl = null;
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            try
            {
                var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "mpsc");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                var fileName = $"{regNumber}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(stream, ct);
                }
                photoUrl = $"/uploads/mpsc/{fileName}";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to upload photo for candidate {RegNumber}", regNumber);
            }
        }

        var candidate = new MpscStudent
        {
            RegistrationNumber = regNumber,
            FullName = model.FullName.Trim(),
            FullNameMarathi = model.FullNameMarathi?.Trim(),
            Email = cleanEmail,
            Mobile = cleanMobile,
            AlternateMobile = !string.IsNullOrWhiteSpace(model.AlternateMobile) ? NormalizeMobile(model.AlternateMobile) : null,
            DateOfBirth = model.DateOfBirth.Trim(),
            Gender = model.Gender.Trim(),
            PhotoUrl = photoUrl,

            Address = model.Address.Trim(),
            City = model.City?.Trim(),
            District = model.District.Trim(),
            State = string.IsNullOrWhiteSpace(model.State) ? "Maharashtra" : model.State.Trim(),
            Pincode = model.Pincode?.Trim(),

            HighestQualification = model.HighestQualification.Trim(),
            DegreeOrCourse = model.DegreeOrCourse?.Trim(),
            University = model.University?.Trim(),
            YearOfPassing = model.YearOfPassing,

            PreferredExam = string.IsNullOrWhiteSpace(model.PreferredExam) ? "Rajyaseva" : model.PreferredExam.Trim(),
            OtherExamInterest = model.OtherExamInterest?.Trim(),
            PreparationLevel = string.IsNullOrWhiteSpace(model.PreparationLevel) ? "Beginner" : model.PreparationLevel.Trim(),
            TargetAttemptYear = model.TargetAttemptYear,
            PreviousAttempts = model.PreviousAttempts,
            NumberOfPreviousAttempts = model.PreviousAttempts ? (model.NumberOfPreviousAttempts ?? 0) : 0,

            Category = string.IsNullOrWhiteSpace(model.Category) ? "Open" : model.Category.Trim(),
            CasteCertificateAvailable = model.CasteCertificateAvailable,
            NonCreamyLayerCertificateAvailable = model.NonCreamyLayerCertificateAvailable,
            EwsCertificateAvailable = model.EwsCertificateAvailable,

            ConsentAccepted = true,
            ConsentAcceptedAt = DateTime.UtcNow,
            ConsentIpAddress = ipAddress,

            IsActive = true,
            RegistrationDate = DateTime.UtcNow
        };

        db.MpscStudents.Add(candidate);
        await db.SaveChangesAsync(ct);

        // Create portal AppUser account for MPSC candidate
        var password = !string.IsNullOrWhiteSpace(model.Password)
            ? model.Password
            : "Mpsc@" + cleanMobile.Substring(Math.Max(0, cleanMobile.Length - 4));

        var appUser = new AppUser
        {
            Username = candidate.RegistrationNumber,
            Email = cleanEmail,
            PasswordHash = PasswordHasher.Hash(password),
            Role = "MpscStudent",
            FullName = candidate.FullName,
            Mobile = cleanMobile,
            PhotoUrl = candidate.PhotoUrl,
            MpscStudentId = candidate.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(appUser);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync("Created", "MpscStudent", candidate.Id, null,
            $"MPSC Candidate registered: {candidate.RegistrationNumber} ({candidate.FullName})",
            candidate.RegistrationNumber, ipAddress, ct);

        return (true, "Registration completed successfully.", candidate, password);
    }

    public async Task<MpscStudent?> GetCandidateByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.MpscStudents
            .Include(s => s.Registrations)
                .ThenInclude(r => r.Exam)
            .Include(s => s.Results)
                .ThenInclude(r => r.Exam)
            .Include(s => s.OnlineAttempts)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<MpscStudent?> GetCandidateByUserIdAsync(int appUserId, CancellationToken ct = default)
    {
        var user = await db.AppUsers.FindAsync([appUserId], ct);
        if (user?.MpscStudentId is null) return null;

        return await db.MpscStudents
            .Include(s => s.Registrations)
                .ThenInclude(r => r.Exam)
            .Include(s => s.Results)
                .ThenInclude(r => r.Exam)
            .Include(s => s.OnlineAttempts)
            .FirstOrDefaultAsync(s => s.Id == user.MpscStudentId.Value, ct);
    }

    public async Task<MpscCandidateDashboardViewModel> GetDashboardAsync(int mpscStudentId, CancellationToken ct = default)
    {
        var candidate = await db.MpscStudents
            .Include(s => s.Registrations)
                .ThenInclude(r => r.Exam)
            .Include(s => s.Results)
                .ThenInclude(r => r.Exam)
            .Include(s => s.OnlineAttempts)
            .FirstOrDefaultAsync(s => s.Id == mpscStudentId, ct)
            ?? throw new InvalidOperationException($"MPSC Candidate #{mpscStudentId} not found.");

        var (percentage, missing) = CalculateProfileCompletion(candidate);

        // Fetch all MPSC eligible exams
        var mpscExams = await db.Exams
            .Where(e => e.IsPublished && (e.Audience == "Mpsc" || e.Audience == "Both"))
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync(ct);

        var registeredExamIds = candidate.Registrations.Select(r => r.ExamId).ToHashSet();
        var now = DateTime.UtcNow;

        var availableCards = new List<MpscExamCardViewModel>();
        foreach (var exam in mpscExams)
        {
            var isReg = registeredExamIds.Contains(exam.Id);
            var start = exam.StartDateTime ?? exam.ExamDate;
            var end = exam.EndDateTime ?? start.AddDays(7);
            var canAttempt = exam.ExamMode == "Online" && now >= start && now <= end && (!exam.IsRegistrationRequired || isReg);
            var hasAttempted = candidate.OnlineAttempts.Any(a => a.ExamId == exam.Id && a.Status is "Submitted" or "AutoSubmitted");
            var result = candidate.Results.FirstOrDefault(r => r.ExamId == exam.Id);

            availableCards.Add(new MpscExamCardViewModel
            {
                ExamId = exam.Id,
                Title = exam.Title,
                Subject = exam.Subject,
                ExamMode = exam.ExamMode,
                Venue = exam.Venue,
                Capacity = exam.Capacity,
                RegisteredCount = await db.MpscExamRegistrations.CountAsync(r => r.ExamId == exam.Id, ct),
                TotalMarks = exam.TotalMarks,
                PassingMarks = exam.PassingMarks,
                DurationMinutes = exam.DurationMinutes > 0 ? exam.DurationMinutes : 30,
                ExamDate = exam.ExamDate,
                StartDateTime = exam.StartDateTime,
                EndDateTime = exam.EndDateTime,
                IsRegistered = isReg,
                CanAttempt = canAttempt && !hasAttempted,
                HasAttempted = hasAttempted,
                ResultId = result?.Id
            });
        }

        var recentResults = candidate.Results
            .OrderByDescending(r => r.CreatedDate)
            .Take(10)
            .Select(r => new MpscResultHistoryItemViewModel
            {
                ExamId = r.ExamId,
                ExamTitle = r.Exam?.Title ?? "Exam",
                Subject = r.Exam?.Subject ?? "General Studies",
                Mode = r.Mode,
                Date = r.Exam?.ExamDate ?? r.CreatedDate,
                MarksObtained = r.MarksObtained,
                TotalMarks = r.Exam?.TotalMarks ?? 100,
                PassingMarks = r.Exam?.PassingMarks ?? 35,
                Rank = r.Rank,
                Grade = r.Grade,
                IsAbsent = r.IsAbsent,
                Remarks = r.Remarks,
                ResultId = r.Id,
                OnlineAttemptId = r.OnlineAttemptId
            })
            .ToList();

        var upcomingCount = mpscExams.Count(e => (e.StartDateTime ?? e.ExamDate) > now);
        var completedCount = candidate.Results.Count;

        return new MpscCandidateDashboardViewModel
        {
            Candidate = candidate,
            ProfileCompletionPercentage = percentage,
            MissingFields = missing,
            AvailableExamsCount = mpscExams.Count,
            RegisteredExamsCount = candidate.Registrations.Count,
            UpcomingExamsCount = upcomingCount,
            CompletedExamsCount = completedCount,
            ResultsCount = candidate.Results.Count,
            AvailableExams = availableCards,
            RecentResults = recentResults
        };
    }

    public async Task<MpscProfileViewModel?> GetProfileAsync(int mpscStudentId, CancellationToken ct = default)
    {
        var c = await db.MpscStudents.FindAsync([mpscStudentId], ct);
        if (c is null) return null;

        return new MpscProfileViewModel
        {
            Id = c.Id,
            RegistrationNumber = c.RegistrationNumber,
            FullName = c.FullName,
            FullNameMarathi = c.FullNameMarathi,
            DateOfBirth = c.DateOfBirth,
            Gender = c.Gender,
            Mobile = c.Mobile,
            AlternateMobile = c.AlternateMobile,
            Email = c.Email,
            CurrentPhotoUrl = c.PhotoUrl,
            Address = c.Address,
            City = c.City,
            District = c.District,
            State = c.State,
            Pincode = c.Pincode,
            HighestQualification = c.HighestQualification,
            DegreeOrCourse = c.DegreeOrCourse,
            University = c.University,
            YearOfPassing = c.YearOfPassing,
            PreferredExam = c.PreferredExam,
            OtherExamInterest = c.OtherExamInterest,
            PreparationLevel = c.PreparationLevel,
            TargetAttemptYear = c.TargetAttemptYear,
            PreviousAttempts = c.PreviousAttempts,
            NumberOfPreviousAttempts = c.NumberOfPreviousAttempts,
            Category = c.Category,
            CasteCertificateAvailable = c.CasteCertificateAvailable,
            NonCreamyLayerCertificateAvailable = c.NonCreamyLayerCertificateAvailable,
            EwsCertificateAvailable = c.EwsCertificateAvailable
        };
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(MpscProfileViewModel model, CancellationToken ct = default)
    {
        var candidate = await db.MpscStudents.FindAsync([model.Id], ct);
        if (candidate is null)
            return (false, "Candidate record not found.");

        var cleanMobile = NormalizeMobile(model.Mobile);
        var cleanEmail = model.Email.Trim().ToLowerInvariant();

        // Check if mobile or email changed and conflicts
        if (cleanMobile != candidate.Mobile)
        {
            var mobileExists = await db.MpscStudents.AnyAsync(s => s.Id != candidate.Id && s.Mobile == cleanMobile, ct);
            if (mobileExists) return (false, "Another candidate is already using this mobile number.");
        }

        if (cleanEmail != candidate.Email)
        {
            var emailExists = await db.MpscStudents.AnyAsync(s => s.Id != candidate.Id && s.Email.ToLower() == cleanEmail, ct);
            if (emailExists) return (false, "Another candidate is already using this email address.");
        }

        // Photo upload
        if (model.NewPhotoFile != null && model.NewPhotoFile.Length > 0)
        {
            try
            {
                var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "mpsc");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(model.NewPhotoFile.FileName).ToLowerInvariant();
                var fileName = $"{candidate.RegistrationNumber}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewPhotoFile.CopyToAsync(stream, ct);
                }
                candidate.PhotoUrl = $"/uploads/mpsc/{fileName}";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to upload photo for candidate {RegNumber}", candidate.RegistrationNumber);
            }
        }

        candidate.FullName = model.FullName.Trim();
        candidate.FullNameMarathi = model.FullNameMarathi?.Trim();
        candidate.DateOfBirth = model.DateOfBirth.Trim();
        candidate.Gender = model.Gender.Trim();
        candidate.Mobile = cleanMobile;
        candidate.AlternateMobile = !string.IsNullOrWhiteSpace(model.AlternateMobile) ? NormalizeMobile(model.AlternateMobile) : null;
        candidate.Email = cleanEmail;

        candidate.Address = model.Address.Trim();
        candidate.City = model.City?.Trim();
        candidate.District = model.District.Trim();
        candidate.State = string.IsNullOrWhiteSpace(model.State) ? "Maharashtra" : model.State.Trim();
        candidate.Pincode = model.Pincode?.Trim();

        candidate.HighestQualification = model.HighestQualification.Trim();
        candidate.DegreeOrCourse = model.DegreeOrCourse?.Trim();
        candidate.University = model.University?.Trim();
        candidate.YearOfPassing = model.YearOfPassing;

        candidate.PreferredExam = model.PreferredExam.Trim();
        candidate.OtherExamInterest = model.OtherExamInterest?.Trim();
        candidate.PreparationLevel = model.PreparationLevel.Trim();
        candidate.TargetAttemptYear = model.TargetAttemptYear;
        candidate.PreviousAttempts = model.PreviousAttempts;
        candidate.NumberOfPreviousAttempts = model.PreviousAttempts ? (model.NumberOfPreviousAttempts ?? 0) : 0;

        candidate.Category = model.Category.Trim();
        candidate.CasteCertificateAvailable = model.CasteCertificateAvailable;
        candidate.NonCreamyLayerCertificateAvailable = model.NonCreamyLayerCertificateAvailable;
        candidate.EwsCertificateAvailable = model.EwsCertificateAvailable;

        candidate.LastUpdatedDate = DateTime.UtcNow;

        // Sync to AppUser account
        var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.MpscStudentId == candidate.Id, ct);
        if (appUser is not null)
        {
            appUser.FullName = candidate.FullName;
            appUser.Mobile = candidate.Mobile;
            appUser.Email = candidate.Email;
            appUser.PhotoUrl = candidate.PhotoUrl;
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Updated", "MpscStudent", candidate.Id, null,
            $"Profile updated for {candidate.RegistrationNumber}", candidate.RegistrationNumber, null, ct);

        return (true, "Profile updated successfully.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        int mpscStudentId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.MpscStudentId == mpscStudentId && u.Role == "MpscStudent", ct);
        if (appUser is null) return (false, "User account not found.");

        if (!PasswordHasher.Verify(currentPassword, appUser.PasswordHash))
            return (false, "The current password is incorrect.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "The new password must be at least 6 characters long.");

        appUser.PasswordHash = PasswordHasher.Hash(newPassword);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync("Updated", "AppUser", appUser.Id, null,
            $"Password changed for MPSC student user {appUser.Username}", appUser.Username, null, ct);

        return (true, "Password changed successfully.");
    }

    // ── Exam Registration & Results for Candidates ───────────────────────────

    public async Task<IReadOnlyList<MpscExamCardViewModel>> GetAvailableExamsAsync(int mpscStudentId, CancellationToken ct = default)
    {
        var candidate = await db.MpscStudents
            .Include(s => s.Registrations)
            .Include(s => s.Results)
            .Include(s => s.OnlineAttempts)
            .FirstOrDefaultAsync(s => s.Id == mpscStudentId, ct)
            ?? throw new InvalidOperationException($"Candidate #{mpscStudentId} not found.");

        var exams = await db.Exams
            .Where(e => e.IsPublished && (e.Audience == "Mpsc" || e.Audience == "Both"))
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync(ct);

        var registeredMap = candidate.Registrations.ToDictionary(r => r.ExamId);
        var resultsMap = candidate.Results.ToDictionary(r => r.ExamId);
        var now = DateTime.UtcNow;

        var list = new List<MpscExamCardViewModel>();
        foreach (var exam in exams)
        {
            var isReg = registeredMap.ContainsKey(exam.Id);
            var start = exam.StartDateTime ?? exam.ExamDate;
            var end = exam.EndDateTime ?? start.AddDays(7);
            var canAttempt = exam.ExamMode == "Online" && now >= start && now <= end && (!exam.IsRegistrationRequired || isReg);
            var hasAttempted = candidate.OnlineAttempts.Any(a => a.ExamId == exam.Id && a.Status is "Submitted" or "AutoSubmitted");
            resultsMap.TryGetValue(exam.Id, out var res);

            list.Add(new MpscExamCardViewModel
            {
                ExamId = exam.Id,
                Title = exam.Title,
                Subject = exam.Subject,
                ExamMode = exam.ExamMode,
                Venue = exam.Venue,
                Capacity = exam.Capacity,
                RegisteredCount = await db.MpscExamRegistrations.CountAsync(r => r.ExamId == exam.Id, ct),
                TotalMarks = exam.TotalMarks,
                PassingMarks = exam.PassingMarks,
                DurationMinutes = exam.DurationMinutes > 0 ? exam.DurationMinutes : 30,
                ExamDate = exam.ExamDate,
                StartDateTime = exam.StartDateTime,
                EndDateTime = exam.EndDateTime,
                IsRegistered = isReg,
                CanAttempt = canAttempt && !hasAttempted,
                HasAttempted = hasAttempted,
                ResultId = res?.Id
            });
        }

        return list;
    }

    public async Task<(bool Success, string Message)> RegisterForExamAsync(int examId, int mpscStudentId, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is null || !exam.IsPublished || (exam.Audience != "Mpsc" && exam.Audience != "Both"))
            return (false, "Exam is not available for MPSC candidates.");

        var candidate = await db.MpscStudents.FindAsync([mpscStudentId], ct);
        if (candidate is null || !candidate.IsActive)
            return (false, "Active candidate profile not found.");

        var existing = await db.MpscExamRegistrations
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.MpscStudentId == mpscStudentId, ct);

        if (existing is not null)
        {
            if (existing.Status == "Cancelled")
            {
                existing.Status = "Registered";
                await db.SaveChangesAsync(ct);
                return (true, "Your exam registration has been reactivated.");
            }
            return (true, "You are already registered for this exam.");
        }

        // Check registration window if defined
        var now = DateTime.UtcNow;
        if (exam.RegistrationOpenDate.HasValue && now < exam.RegistrationOpenDate.Value)
            return (false, $"Registration opens on {exam.RegistrationOpenDate.Value.ToLocalTime():dd MMM yyyy HH:mm}.");

        if (exam.RegistrationCloseDate.HasValue && now > exam.RegistrationCloseDate.Value)
            return (false, "Registration deadline has closed.");

        // Check venue capacity for offline exams
        string? seatNumber = null;
        if (exam.ExamMode == "Offline")
        {
            var registeredCount = await db.MpscExamRegistrations.CountAsync(r => r.ExamId == examId && r.Status != "Cancelled", ct);
            if (exam.Capacity.HasValue && registeredCount >= exam.Capacity.Value)
                return (false, $"This exam center is full. Maximum seating capacity of {exam.Capacity.Value} has been reached.");

            seatNumber = $"SEAT-{exam.Id}-{(registeredCount + 1):D3}";
        }

        var reg = new MpscExamRegistration
        {
            ExamId = examId,
            MpscStudentId = mpscStudentId,
            RegisteredAt = DateTime.UtcNow,
            Status = "Registered",
            Mode = exam.ExamMode,
            SeatNumber = seatNumber,
            AttendanceStatus = "Pending"
        };

        db.MpscExamRegistrations.Add(reg);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync("Created", "MpscExamRegistration", reg.Id, null,
            $"Candidate #{candidate.RegistrationNumber} registered for Exam #{exam.Id} ({exam.Title})",
            candidate.RegistrationNumber, null, ct);

        return (true, "You have been successfully registered for this exam.");
    }

    public async Task<IReadOnlyList<MpscResultHistoryItemViewModel>> GetCandidateResultsAsync(int mpscStudentId, CancellationToken ct = default)
    {
        var results = await db.MpscExamResults
            .Include(r => r.Exam)
            .Where(r => r.MpscStudentId == mpscStudentId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(ct);

        return results.Select(r => new MpscResultHistoryItemViewModel
        {
            ExamId = r.ExamId,
            ExamTitle = r.Exam?.Title ?? "Exam",
            Subject = r.Exam?.Subject ?? "General Studies",
            Mode = r.Mode,
            Date = r.Exam?.ExamDate ?? r.CreatedDate,
            MarksObtained = r.MarksObtained,
            TotalMarks = r.Exam?.TotalMarks ?? 100,
            PassingMarks = r.Exam?.PassingMarks ?? 35,
            Rank = r.Rank,
            Grade = r.Grade,
            IsAbsent = r.IsAbsent,
            Remarks = r.Remarks,
            ResultId = r.Id,
            OnlineAttemptId = r.OnlineAttemptId
        }).ToList();
    }

    // ── Admin Operations ─────────────────────────────────────────────────────

    public async Task<MpscAdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var total = await db.MpscStudents.CountAsync(ct);
        var active = await db.MpscStudents.CountAsync(s => s.IsActive, ct);
        var inactive = total - active;

        var todayStart = UtcDateTime.StartOfToday();
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todayCount = await db.MpscStudents.CountAsync(s => s.RegistrationDate >= todayStart, ct);
        var monthCount = await db.MpscStudents.CountAsync(s => s.RegistrationDate >= monthStart, ct);

        var onlineRegs = await db.MpscExamRegistrations.CountAsync(r => r.Mode == "Online", ct);
        var offlineRegs = await db.MpscExamRegistrations.CountAsync(r => r.Mode == "Offline", ct);

        var mpscExams = await db.Exams
            .Where(e => e.Audience == "Mpsc" || e.Audience == "Both")
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var upcoming = mpscExams.Count(e => (e.StartDateTime ?? e.ExamDate) > now);
        var completed = mpscExams.Count(e => (e.EndDateTime ?? e.ExamDate.AddDays(1)) < now);

        var pendingResults = await db.MpscExamRegistrations
            .CountAsync(r => r.Mode == "Offline" && r.AttendanceStatus == "Present" &&
                             !db.MpscExamResults.Any(res => res.ExamId == r.ExamId && res.MpscStudentId == r.MpscStudentId), ct);

        var recentCandidates = await db.MpscStudents
            .OrderByDescending(s => s.RegistrationDate)
            .Take(10)
            .ToListAsync(ct);

        var upcomingExamCards = mpscExams
            .Where(e => (e.StartDateTime ?? e.ExamDate) >= now)
            .OrderBy(e => e.StartDateTime ?? e.ExamDate)
            .Take(5)
            .Select(e => new MpscExamCardViewModel
            {
                ExamId = e.Id,
                Title = e.Title,
                Subject = e.Subject,
                ExamMode = e.ExamMode,
                Venue = e.Venue,
                Capacity = e.Capacity,
                TotalMarks = e.TotalMarks,
                PassingMarks = e.PassingMarks,
                DurationMinutes = e.DurationMinutes > 0 ? e.DurationMinutes : 30,
                ExamDate = e.ExamDate,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime
            })
            .ToList();

        return new MpscAdminDashboardViewModel
        {
            TotalCandidates = total,
            ActiveCandidates = active,
            InactiveCandidates = inactive,
            NewRegistrationsToday = todayCount,
            NewRegistrationsThisMonth = monthCount,
            OnlineExamRegistrations = onlineRegs,
            OfflineExamRegistrations = offlineRegs,
            UpcomingMpscExamsCount = upcoming,
            CompletedMpscExamsCount = completed,
            PendingResultsCount = pendingResults,
            RecentCandidates = recentCandidates,
            UpcomingExams = upcomingExamCards
        };
    }

    public async Task<MpscCandidateListFilterViewModel> GetCandidatesAsync(MpscCandidateListFilterViewModel filter, CancellationToken ct = default)
    {
        var query = db.MpscStudents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.RegistrationNumber.ToLower().Contains(s) ||
                c.FullName.ToLower().Contains(s) ||
                c.Email.ToLower().Contains(s) ||
                c.Mobile.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (filter.Status == "Active") query = query.Where(c => c.IsActive);
            else if (filter.Status == "Inactive") query = query.Where(c => !c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.ExamInterest))
        {
            query = query.Where(c => c.PreferredExam == filter.ExamInterest);
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(c => c.District == filter.District);
        }

        if (filter.TargetYear.HasValue)
        {
            query = query.Where(c => c.TargetAttemptYear == filter.TargetYear.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PreparationLevel))
        {
            query = query.Where(c => c.PreparationLevel == filter.PreparationLevel);
        }

        filter.TotalRecords = await query.CountAsync(ct);

        filter.Candidates = await query
            .OrderByDescending(c => c.RegistrationDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        filter.AvailableDistricts = await db.MpscStudents
            .Select(c => c.District)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(ct);

        return filter;
    }

    public async Task<MpscCandidateDetailViewModel?> GetCandidateDetailAsync(int candidateId, CancellationToken ct = default)
    {
        var candidate = await db.MpscStudents
            .Include(s => s.Registrations)
                .ThenInclude(r => r.Exam)
            .Include(s => s.OnlineAttempts)
                .ThenInclude(a => a.Exam)
            .Include(s => s.Results)
                .ThenInclude(r => r.Exam)
            .FirstOrDefaultAsync(s => s.Id == candidateId, ct);

        if (candidate is null) return null;

        var userAccount = await db.AppUsers.FirstOrDefaultAsync(u => u.MpscStudentId == candidateId, ct);

        return new MpscCandidateDetailViewModel
        {
            Candidate = candidate,
            UserAccount = userAccount,
            Registrations = candidate.Registrations.OrderByDescending(r => r.RegisteredAt).ToList(),
            OnlineAttempts = candidate.OnlineAttempts.OrderByDescending(a => a.StartedAt).ToList(),
            Results = candidate.Results.OrderByDescending(r => r.CreatedDate).ToList()
        };
    }

    public async Task<(bool Success, string Message)> ToggleCandidateStatusAsync(int candidateId, CancellationToken ct = default)
    {
        var candidate = await db.MpscStudents.FindAsync([candidateId], ct);
        if (candidate is null) return (false, "Candidate not found.");

        candidate.IsActive = !candidate.IsActive;

        var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.MpscStudentId == candidateId, ct);
        if (appUser is not null)
        {
            appUser.IsActive = candidate.IsActive;
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Updated", "MpscStudent", candidate.Id, null,
            $"Toggled candidate status to {(candidate.IsActive ? "Active" : "Inactive")}",
            candidate.RegistrationNumber, null, ct);

        return (true, $"Candidate is now {(candidate.IsActive ? "Active" : "Inactive")}.");
    }

    public async Task<(bool Success, string Message)> ResetCandidatePasswordAsync(int candidateId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "Password must be at least 6 characters.");

        var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.MpscStudentId == candidateId, ct);
        if (appUser is null) return (false, "User account not found.");

        appUser.PasswordHash = PasswordHasher.Hash(newPassword);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync("Updated", "AppUser", appUser.Id, null,
            $"Admin reset password for MPSC candidate #{candidateId}", appUser.Username, null, ct);

        return (true, "Password has been successfully reset.");
    }

    public async Task<byte[]> ExportCandidatesCsvAsync(MpscCandidateListFilterViewModel filter, CancellationToken ct = default)
    {
        var query = db.MpscStudents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.RegistrationNumber.ToLower().Contains(s) ||
                c.FullName.ToLower().Contains(s) ||
                c.Email.ToLower().Contains(s) ||
                c.Mobile.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (filter.Status == "Active") query = query.Where(c => c.IsActive);
            else if (filter.Status == "Inactive") query = query.Where(c => !c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.ExamInterest))
            query = query.Where(c => c.PreferredExam == filter.ExamInterest);

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(c => c.District == filter.District);

        var list = await query.OrderByDescending(c => c.RegistrationDate).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("RegistrationNumber,FullName,FullNameMarathi,Email,Mobile,AlternateMobile,DateOfBirth,Gender,District,HighestQualification,DegreeOrCourse,PreferredExam,PreparationLevel,TargetAttemptYear,Category,IsActive,RegistrationDate");

        foreach (var c in list)
        {
            sb.AppendLine($"\"{c.RegistrationNumber}\",\"{c.FullName}\",\"{c.FullNameMarathi}\",\"{c.Email}\",\"{c.Mobile}\",\"{c.AlternateMobile}\",\"{c.DateOfBirth}\",\"{c.Gender}\",\"{c.District}\",\"{c.HighestQualification}\",\"{c.DegreeOrCourse}\",\"{c.PreferredExam}\",\"{c.PreparationLevel}\",{c.TargetAttemptYear},\"{c.Category}\",{c.IsActive},\"{c.RegistrationDate:yyyy-MM-dd}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<MpscImportPreviewViewModel> PreviewImportCsvAsync(Stream csvStream, CancellationToken ct = default)
    {
        var preview = new MpscImportPreviewViewModel();
        using var reader = new StreamReader(csvStream, Encoding.UTF8);

        string? headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine))
            return preview;

        int rowNumber = 1;
        var existingMobiles = (await db.MpscStudents.Select(s => s.Mobile).ToListAsync(ct)).ToHashSet();
        var existingEmails = (await db.MpscStudents.Select(s => s.Email.ToLower()).ToListAsync(ct)).ToHashSet();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            rowNumber++;

            var parts = ParseCsvRow(line);
            if (parts.Count < 4)
            {
                preview.InvalidRows.Add(new MpscImportRow
                {
                    RowNumber = rowNumber,
                    Status = "Invalid",
                    Reason = "Insufficient columns. Expected at least FullName, Mobile, Email, District."
                });
                continue;
            }

            var fullName = parts[0].Trim();
            var mobile = NormalizeMobile(parts[1].Trim());
            var email = parts[2].Trim().ToLowerInvariant();
            var district = parts.Count > 3 ? parts[3].Trim() : "Maharashtra";
            var qualification = parts.Count > 4 ? parts[4].Trim() : "Graduate";
            var examInterest = parts.Count > 5 ? parts[5].Trim() : "Rajyaseva";
            int? targetYear = parts.Count > 6 && int.TryParse(parts[6].Trim(), out var y) ? y : DateTime.UtcNow.Year;

            var rowItem = new MpscImportRow
            {
                RowNumber = rowNumber,
                FullName = fullName,
                Mobile = mobile,
                Email = email,
                District = district,
                Qualification = qualification,
                ExamInterest = examInterest,
                TargetYear = targetYear
            };

            if (string.IsNullOrWhiteSpace(fullName) || mobile.Length != 10 || string.IsNullOrWhiteSpace(email))
            {
                rowItem.Status = "Invalid";
                rowItem.Reason = "Invalid full name, mobile (must be 10 digits), or email.";
                preview.InvalidRows.Add(rowItem);
            }
            else if (existingMobiles.Contains(mobile) || existingEmails.Contains(email))
            {
                rowItem.Status = "Duplicate";
                rowItem.Reason = "Mobile number or Email already exists in the system.";
                preview.DuplicateRows.Add(rowItem);
            }
            else
            {
                rowItem.Status = "Valid";
                preview.ValidRows.Add(rowItem);
                // Temporarily add to prevent intra-file duplicates
                existingMobiles.Add(mobile);
                existingEmails.Add(email);
            }
        }

        preview.TotalRecords = preview.ValidCount + preview.DuplicateCount + preview.InvalidCount;
        return preview;
    }

    public async Task<(int ImportedCount, int SkippedCount, List<string> Errors)> CommitImportAsync(List<MpscImportRow> rows, CancellationToken ct = default)
    {
        int imported = 0;
        int skipped = 0;
        var errors = new List<string>();

        var year = DateTime.UtcNow.Year;
        var prefix = $"SK-MPSC-{year}-";
        var lastCandidate = await db.MpscStudents
            .Where(s => s.RegistrationNumber.StartsWith(prefix))
            .OrderByDescending(s => s.RegistrationNumber)
            .Select(s => s.RegistrationNumber)
            .FirstOrDefaultAsync(ct);

        int seq = 1;
        if (!string.IsNullOrWhiteSpace(lastCandidate) && lastCandidate.Length >= prefix.Length + 5)
        {
            var seqStr = lastCandidate.Substring(prefix.Length);
            if (int.TryParse(seqStr, out var parsedSeq))
                seq = parsedSeq + 1;
        }

        foreach (var row in rows.Where(r => r.Status == "Valid"))
        {
            try
            {
                var cleanMobile = NormalizeMobile(row.Mobile);
                var cleanEmail = row.Email.Trim().ToLowerInvariant();

                if (await db.MpscStudents.AnyAsync(s => s.Mobile == cleanMobile || s.Email.ToLower() == cleanEmail, ct))
                {
                    skipped++;
                    continue;
                }

                var regNumber = $"{prefix}{seq++:D5}";

                var candidate = new MpscStudent
                {
                    RegistrationNumber = regNumber,
                    FullName = row.FullName.Trim(),
                    Email = cleanEmail,
                    Mobile = cleanMobile,
                    DateOfBirth = "2000-01-01",
                    Gender = "Male",
                    Address = row.District.Trim(),
                    District = row.District.Trim(),
                    State = "Maharashtra",
                    HighestQualification = string.IsNullOrWhiteSpace(row.Qualification) ? "Graduate" : row.Qualification.Trim(),
                    PreferredExam = string.IsNullOrWhiteSpace(row.ExamInterest) ? "Rajyaseva" : row.ExamInterest.Trim(),
                    TargetAttemptYear = row.TargetYear,
                    ConsentAccepted = true,
                    ConsentAcceptedAt = DateTime.UtcNow,
                    IsActive = true,
                    RegistrationDate = DateTime.UtcNow
                };

                db.MpscStudents.Add(candidate);
                await db.SaveChangesAsync(ct);

                var password = "Mpsc@" + cleanMobile.Substring(Math.Max(0, cleanMobile.Length - 4));
                var user = new AppUser
                {
                    Username = candidate.RegistrationNumber,
                    Email = cleanEmail,
                    PasswordHash = PasswordHasher.Hash(password),
                    Role = "MpscStudent",
                    FullName = candidate.FullName,
                    Mobile = cleanMobile,
                    MpscStudentId = candidate.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                db.AppUsers.Add(user);
                await db.SaveChangesAsync(ct);
                imported++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing row #{RowNumber} ({Name})", row.RowNumber, row.FullName);
                errors.Add($"Row {row.RowNumber} ({row.FullName}): {ex.Message}");
                skipped++;
            }
        }

        return (imported, skipped, errors);
    }

    // ── Offline Exam Check-In & Marks Entry ──────────────────────────────────

    public async Task<IReadOnlyList<Exam>> GetMpscExamsForAdminAsync(CancellationToken ct = default)
    {
        return await db.Exams
            .Where(e => e.Audience == "Mpsc" || e.Audience == "Both")
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync(ct);
    }

    public async Task<OfflineCheckInViewModel?> GetOfflineCheckInListAsync(int examId, string? search = null, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is null) return null;

        var query = db.MpscExamRegistrations
            .Include(r => r.MpscStudent)
            .Where(r => r.ExamId == examId && r.Status != "Cancelled");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(r =>
                r.MpscStudent!.RegistrationNumber.ToLower().Contains(s) ||
                r.MpscStudent.FullName.ToLower().Contains(s) ||
                r.MpscStudent.Mobile.Contains(s) ||
                (r.SeatNumber != null && r.SeatNumber.ToLower().Contains(s)));
        }

        var regs = await query.OrderBy(r => r.SeatNumber ?? r.MpscStudent!.RegistrationNumber).ToListAsync(ct);

        var presentCount = await db.MpscExamRegistrations.CountAsync(r => r.ExamId == examId && r.AttendanceStatus == "Present", ct);
        var absentCount = await db.MpscExamRegistrations.CountAsync(r => r.ExamId == examId && r.AttendanceStatus == "Absent", ct);

        return new OfflineCheckInViewModel
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            Subject = exam.Subject,
            ExamDate = exam.ExamDate,
            Venue = exam.Venue,
            Capacity = exam.Capacity,
            RegisteredCount = regs.Count,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            SearchTerm = search,
            Participants = regs.Select(r => new OfflineParticipantCheckInItem
            {
                RegistrationId = r.Id,
                MpscStudentId = r.MpscStudentId,
                RegistrationNumber = r.MpscStudent?.RegistrationNumber ?? "",
                FullName = r.MpscStudent?.FullName ?? "",
                Mobile = r.MpscStudent?.Mobile ?? "",
                SeatNumber = r.SeatNumber,
                AttendanceStatus = r.AttendanceStatus,
                AttendanceMarkedAt = r.AttendanceMarkedAt,
                Remarks = r.AttendanceRemarks
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message)> UpdateOfflineAttendanceAsync(
        int registrationId, string status, string? seatNumber, string? remarks, CancellationToken ct = default)
    {
        var reg = await db.MpscExamRegistrations
            .Include(r => r.MpscStudent)
            .FirstOrDefaultAsync(r => r.Id == registrationId, ct);

        if (reg is null) return (false, "Registration record not found.");

        reg.AttendanceStatus = status;
        reg.AttendanceMarkedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(seatNumber)) reg.SeatNumber = seatNumber.Trim();
        reg.AttendanceRemarks = remarks?.Trim();

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Updated", "MpscExamRegistration", reg.Id, null,
            $"Attendance set to {status} for {reg.MpscStudent?.RegistrationNumber}", null, null, ct);

        return (true, "Attendance updated successfully.");
    }

    public async Task<OfflineResultEntryViewModel?> GetOfflineResultEntryModelAsync(int examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is null) return null;

        var regs = await db.MpscExamRegistrations
            .Include(r => r.MpscStudent)
            .Where(r => r.ExamId == examId && r.Status != "Cancelled")
            .OrderBy(r => r.SeatNumber ?? r.MpscStudent!.RegistrationNumber)
            .ToListAsync(ct);

        var existingResults = await db.MpscExamResults
            .Where(r => r.ExamId == examId)
            .ToDictionaryAsync(r => r.MpscStudentId, ct);

        var rows = new List<OfflineResultEntryRow>();
        foreach (var reg in regs)
        {
            if (reg.MpscStudent is null) continue;

            existingResults.TryGetValue(reg.MpscStudentId, out var res);

            rows.Add(new OfflineResultEntryRow
            {
                MpscStudentId = reg.MpscStudentId,
                RegistrationNumber = reg.MpscStudent.RegistrationNumber,
                FullName = reg.MpscStudent.FullName,
                SeatNumber = reg.SeatNumber,
                MarksObtained = res?.MarksObtained,
                Rank = res?.Rank,
                Grade = res?.Grade,
                IsAbsent = res?.IsAbsent ?? (reg.AttendanceStatus == "Absent"),
                Remarks = res?.Remarks ?? reg.AttendanceRemarks
            });
        }

        return new OfflineResultEntryViewModel
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            Subject = exam.Subject,
            TotalMarks = exam.TotalMarks,
            PassingMarks = exam.PassingMarks,
            ExamDate = exam.ExamDate,
            Rows = rows
        };
    }

    public async Task<(bool Success, string Message)> SaveOfflineResultsAsync(OfflineResultEntryViewModel model, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([model.ExamId], ct);
        if (exam is null) return (false, "Exam not found.");

        var existingResults = await db.MpscExamResults
            .Where(r => r.ExamId == model.ExamId)
            .ToDictionaryAsync(r => r.MpscStudentId, ct);

        // Compute rankings based on MarksObtained descending
        var validScores = model.Rows
            .Where(r => !r.IsAbsent && r.MarksObtained.HasValue)
            .OrderByDescending(r => r.MarksObtained!.Value)
            .ToList();

        var ranks = new Dictionary<int, int>();
        int currentRank = 1;
        for (int i = 0; i < validScores.Count; i++)
        {
            if (i > 0 && validScores[i].MarksObtained < validScores[i - 1].MarksObtained)
                currentRank = i + 1;
            ranks[validScores[i].MpscStudentId] = currentRank;
        }

        foreach (var row in model.Rows)
        {
            existingResults.TryGetValue(row.MpscStudentId, out var result);

            double? marks = row.IsAbsent ? null : row.MarksObtained;
            int? rank = ranks.TryGetValue(row.MpscStudentId, out var r) ? r : null;
            string? grade = null;

            if (marks.HasValue && exam.TotalMarks > 0)
            {
                var pct = (marks.Value / exam.TotalMarks) * 100;
                grade = CalculateGrade(pct);
            }

            if (result is null)
            {
                db.MpscExamResults.Add(new MpscExamResult
                {
                    ExamId = model.ExamId,
                    MpscStudentId = row.MpscStudentId,
                    MarksObtained = marks,
                    Rank = rank,
                    Grade = grade,
                    IsAbsent = row.IsAbsent,
                    Remarks = row.Remarks?.Trim(),
                    Mode = "Offline",
                    CreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                result.MarksObtained = marks;
                result.Rank = rank;
                result.Grade = grade;
                result.IsAbsent = row.IsAbsent;
                result.Remarks = row.Remarks?.Trim();
                result.Mode = "Offline";
            }
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Updated", "MpscExamResult", model.ExamId, null,
            $"Saved offline results for Exam #{model.ExamId} ({model.Rows.Count} entries)", null, null, ct);

        return (true, "Exam marks saved and rankings computed successfully.");
    }

    // ── Helper Utilities ─────────────────────────────────────────────────────

    private static (int Percentage, List<string> Missing) CalculateProfileCompletion(MpscStudent s)
    {
        var missing = new List<string>();
        int totalPoints = 10;
        int earned = 0;

        if (!string.IsNullOrWhiteSpace(s.FullName)) earned++; else missing.Add("Full Name");
        if (!string.IsNullOrWhiteSpace(s.Email)) earned++; else missing.Add("Email");
        if (!string.IsNullOrWhiteSpace(s.Mobile)) earned++; else missing.Add("Mobile");
        if (!string.IsNullOrWhiteSpace(s.DateOfBirth)) earned++; else missing.Add("Date of Birth");
        if (!string.IsNullOrWhiteSpace(s.Address)) earned++; else missing.Add("Address");
        if (!string.IsNullOrWhiteSpace(s.District)) earned++; else missing.Add("District");
        if (!string.IsNullOrWhiteSpace(s.HighestQualification)) earned++; else missing.Add("Education Qualification");
        if (!string.IsNullOrWhiteSpace(s.PreferredExam)) earned++; else missing.Add("Target Exam");
        if (!string.IsNullOrWhiteSpace(s.PhotoUrl)) earned++; else missing.Add("Profile Photo");
        if (!string.IsNullOrWhiteSpace(s.Category)) earned++; else missing.Add("Category");

        int pct = (earned * 100) / totalPoints;
        return (pct, missing);
    }

    private static string NormalizeMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return string.Empty;
        var digits = new string(mobile.Where(char.IsDigit).ToArray());
        return digits.Length > 10 ? digits[^10..] : digits;
    }

    private static string CalculateGrade(double percentage) => percentage switch
    {
        >= 80 => "A+",
        >= 70 => "A",
        >= 60 => "B+",
        >= 50 => "B",
        >= 35 => "C",
        _ => "F"
    };

    private static List<string> ParseCsvRow(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    current.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}
