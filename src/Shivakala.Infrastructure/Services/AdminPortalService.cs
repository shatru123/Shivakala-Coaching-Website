using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shivakala.Infrastructure.Services;

public sealed class AdminPortalService(
    IStudentRepository studentRepository,
    IEnquiryRepository enquiryRepository,
    INoticeRepository noticeRepository,
    IStudyMaterialRepository materialRepository,
    IPortalUserService portalUsers,
    ShivakalaDbContext db) : IAdminPortalService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var registrations = await GetRegistrationsAsync(ct);
        var enquiries = await GetEnquiriesAsync(ct);
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return new AdminDashboardViewModel
        {
            RegistrationCount = registrations.Count,
            EnquiryCount = enquiries.Count,
            UnreadEnquiries = enquiries.Count(e => !e.IsRead),
            PendingAdmissions = registrations.Count(r => r.Status == "Pending"),
            NoticeCount = (await noticeRepository.GetAllAdminAsync(ct)).Count,
            MaterialCount = (await materialRepository.GetAllAdminAsync(ct)).Count,
            RecentRegistrations = registrations.Take(5).ToList(),
            RecentEnquiries = enquiries.Take(5).ToList(),
            NewRegistrationsThisMonth = registrations.Count(r => r.CreatedDate >= monthStart),
            NewEnquiriesThisMonth = enquiries.Count(e => e.CreatedDate >= monthStart)
        };
    }

    public async Task<IReadOnlyList<StudentAdminViewModel>> GetRegistrationsAsync(CancellationToken ct = default)
    {
        var students = await studentRepository.ListRecentAsync(ct);
        return students.Select(x => new StudentAdminViewModel
        {
            Id = x.Id, FullName = x.FullName, ParentName = x.ParentName, Mobile = x.Mobile,
            Email = x.Email, Standard = x.Standard, Subject = x.Subject, Address = x.Address,
            Board = x.Board, Medium = x.Medium, AdmissionNumber = x.AdmissionNumber, PhotoUrl = x.PhotoUrl,
            Status = x.Status, AdminNotes = x.AdminNotes,
            CreatedDate = x.CreatedDate
        }).ToList();
    }

    public async Task<IReadOnlyList<EnquiryAdminViewModel>> GetEnquiriesAsync(CancellationToken ct = default)
    {
        var enquiries = await enquiryRepository.ListRecentAsync(ct);
        return enquiries.Select(x => new EnquiryAdminViewModel
        {
            Id = x.Id, Name = x.Name, Mobile = x.Mobile, Email = x.Email,
            Message = x.Message, ClassInterested = x.ClassInterested,
            IsRead = x.IsRead, AdminReply = x.AdminReply, CreatedDate = x.CreatedDate
        }).ToList();
    }

    public async Task<int> CreateStudentAsync(AdminStudentFormViewModel model, CancellationToken ct = default)
    {
        var student = new Core.Entities.Student
        {
            FullName    = model.FullName.Trim(),
            ParentName  = string.IsNullOrWhiteSpace(model.ParentName) ? null : model.ParentName.Trim(),
            Mobile      = model.Mobile.Trim(),
            Email       = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Standard    = model.Standard.Trim(),
            Subject     = model.Subject.Trim(),
            Address     = model.Address.Trim(),
            Board       = string.IsNullOrWhiteSpace(model.Board) ? null : model.Board.Trim(),
            Medium      = string.IsNullOrWhiteSpace(model.Medium) ? null : model.Medium.Trim(),
            ParentMobile = string.IsNullOrWhiteSpace(model.ParentMobile) ? null : model.ParentMobile.Trim(),
            Status      = model.Status.Trim(),
            AdminNotes  = string.IsNullOrWhiteSpace(model.AdminNotes) ? null : model.AdminNotes.Trim(),
            CreatedDate = DateTime.UtcNow
        };
        await studentRepository.AddAsync(student, ct);
        student.AdmissionNumber = $"SK{DateTime.UtcNow:yyyy}{student.Id:D4}";
        await db.SaveChangesAsync(ct);

        if (model.Status == "Admitted")
            await portalUsers.EnsureParentAccountForStudentAsync(student.Id, ct: ct);

        return student.Id;
    }

    public async Task UpdateStudentStatusAsync(int id, string status, string? notes, CancellationToken ct = default)
    {
        var student = await db.Students.FindAsync([id], ct);
        if (student != null)
        {
            student.Status = status;
            if (notes != null) student.AdminNotes = notes;
            await db.SaveChangesAsync(ct);
            if (status == "Admitted")
                await portalUsers.EnsureParentAccountForStudentAsync(id, ct: ct);
        }
    }

    public async Task MarkEnquiryReadAsync(int id, string? reply, CancellationToken ct = default)
    {
        var enquiry = await db.Enquiries.FindAsync([id], ct);
        if (enquiry != null)
        {
            enquiry.IsRead = true;
            if (reply != null) enquiry.AdminReply = reply;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<byte[]> ExportRegistrationsCsvAsync(CancellationToken ct = default)
    {
        var students = await db.Students.OrderByDescending(s => s.CreatedDate).ToListAsync(ct);
        var lines = new List<string> { "Id,FullName,ParentName,Mobile,Email,Standard,Subject,Board,Medium,Status,Address,CreatedDate" };
        lines.AddRange(students.Select(s =>
            $"{s.Id},\"{s.FullName}\",\"{s.ParentName??""}\",{s.Mobile},{s.Email??""},\"{s.Standard}\",\"{s.Subject}\",\"{s.Board??""}\",\"{s.Medium??""}\",{s.Status},\"{s.Address}\",{s.CreatedDate:yyyy-MM-dd HH:mm}"));
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }

    public async Task<byte[]> ExportEnquiriesCsvAsync(CancellationToken ct = default)
    {
        var enquiries = await db.Enquiries.OrderByDescending(e => e.CreatedDate).ToListAsync(ct);
        var lines = new List<string> { "Id,Name,Mobile,Email,ClassInterested,IsRead,Message,CreatedDate" };
        lines.AddRange(enquiries.Select(e =>
            $"{e.Id},\"{e.Name}\",{e.Mobile},{e.Email??""},\"{e.ClassInterested??""}\",{e.IsRead},\"{e.Message.Replace("\"","'")}\",{e.CreatedDate:yyyy-MM-dd HH:mm}"));
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }
}
