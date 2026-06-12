using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Common;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

// ── CLASS-LEVEL ROLE GUARD ────────────────────────────────────────────────────
// Applies [Authorize(Roles = "Admin")] to EVERY action in this controller.
// Teachers and Parents are authenticated but do NOT have this role, so ASP.NET
// Core returns HTTP 403 → OnRedirectToAccessDenied fires → redirects them to
// their own portal dashboard. No admin page is ever rendered for other roles.
[Authorize(Roles = "Admin")]
public sealed class AdminController(
    IAdminAuthenticationService authService,
    IPortalUserService portalUsers,
    IAdminPortalService portalService,
    ITeacherRepository teacherRepo,
    ICourseRepository courseRepo,
    INoticeRepository noticeRepo,
    ITestResultRepository resultRepo,
    IStudyMaterialRepository materialRepo,
    IGalleryRepository galleryRepo,
    ITestimonialRepository testimonialRepo,
    ShivakalaDbContext db,
    IWebHostEnvironment webHostEnvironment,
    ILogger<AdminController> logger) : Controller
{
    // ═══ AUTH ═════════════════════════════════════════════════════════════════
    // [AllowAnonymous] overrides the class-level [Authorize] for login actions only.

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already authenticated — send each role to its own portal
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))   return RedirectToAction(nameof(Index));
            if (User.IsInRole("Teacher")) return Redirect("/teacher");
            if (User.IsInRole("Parent"))  return Redirect("/parent");
        }
        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(AdminLoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Login", model);
        var user = await authService.ValidateCredentialsAsync(model.Username, model.Password, ct);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View("Login", model);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName ?? user.Username),
            new(ClaimTypes.Role, "Admin"),
            new("UserId", user.Id.ToString())
        };
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Admin logged in: {User}", user.Username);
        return !string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl) : RedirectToAction(nameof(Index));
    }

    // Logout — admin-only (teachers/parents have their own /teacher/logout, /parent/logout)
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        logger.LogInformation("Admin signed out: {User}", User.Identity?.Name);
        return RedirectToAction(nameof(Login));
    }

    // ═══ DASHBOARD ════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dashboard = await portalService.GetDashboardAsync(ct);
        var currentMonth = UtcDateTime.CurrentMonthKey();
        var todayUtc = UtcDateTime.StartOfToday();

        ViewBag.TotalStudents   = await db.Students.CountAsync(ct);
        ViewBag.TotalTeachers   = await db.Teachers.CountAsync(s => s.IsActive, ct);
        ViewBag.TotalBatches    = await db.Batches.CountAsync(b => b.IsActive, ct);
        ViewBag.TotalEnquiries  = await db.Enquiries.CountAsync(e => !e.IsRead, ct);
        ViewBag.FeeThisMonth    = (decimal)(await db.FeePayments
            .Where(f => f.Month == currentMonth && f.Status == "Paid")
            .SumAsync(f => (double?)f.PaidAmount, ct) ?? 0);
        ViewBag.PendingFees     = (decimal)(await db.FeePayments
            .Where(f => f.Status == "Pending")
            .SumAsync(f => (double?)f.Amount - (double?)f.PaidAmount, ct) ?? 0);
        ViewBag.AttendanceToday = await db.Attendances
            .Where(a => a.Date == UtcDateTime.Today())
            .Select(a => a.StudentId)
            .Distinct()
            .CountAsync(ct);
        ViewBag.UpcomingExams   = await db.Exams
            .Where(e => e.ExamDate >= todayUtc && !e.IsPublished).CountAsync(ct);
        ViewBag.PendingHomework = await db.Homeworks
            .Where(h => h.IsActive && h.DueDate >= todayUtc).CountAsync(ct);

        return View(dashboard);
    }

    // ═══ REGISTRATIONS ════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Registrations(string? status, string? search, CancellationToken ct)
    {
        var all = await portalService.GetRegistrationsAsync(ct);
        if (!string.IsNullOrWhiteSpace(status))
            all = all.Where(r => r.Status == status).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(r => r.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || r.Mobile.Contains(search)).ToList();
        ViewData["Status"] = status;
        ViewData["Search"] = search;
        return View(new AdminListPageViewModel<StudentAdminViewModel> { Title = "Registrations", Items = all });
    }

    [HttpGet]
    public IActionResult CreateStudent() => View("StudentForm", new AdminStudentFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudent(AdminStudentFormViewModel vm, IFormFile? photo, CancellationToken ct)
    {
        if (photo is { Length: > 0 })
        {
            try
            {
                ValidateImageUpload(photo);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        if (!ModelState.IsValid) return View("StudentForm", vm);
        var id = await portalService.CreateStudentAsync(vm, ct);
        if (photo is { Length: > 0 })
        {
            var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (student is not null)
            {
                student.PhotoUrl = await SaveImageUploadAsync(photo, "students", null, ct);
                await db.SaveChangesAsync(ct);
            }
        }
        TempData["SuccessMessage"] = $"Student {vm.FullName} added successfully (ID #{id}).";
        return RedirectToAction(nameof(Registrations));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudentStatus(int id, string status, string? notes, CancellationToken ct)
    {
        await portalService.UpdateStudentStatusAsync(id, status, notes, ct);
        TempData["SuccessMessage"] = $"Student #{id} status updated to {status}.";
        return RedirectToAction(nameof(Registrations));
    }

    [HttpGet]
    public async Task<IActionResult> ExportRegistrations(CancellationToken ct)
    {
        var csv = await portalService.ExportRegistrationsCsvAsync(ct);
        return File(csv, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> StudentIdCard(int id, CancellationToken ct)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) return NotFound();

        var hasChanges = EnsureAdmissionNumber(student);
        if (hasChanges)
            await db.SaveChangesAsync(ct);

        return View(MapStudentIdCard(student));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StudentIdCard(StudentIdCardAdminViewModel vm, IFormFile? photo, CancellationToken ct)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == vm.Id, ct);
        if (student is null) return NotFound();

        EnsureAdmissionNumber(student);

        if (!ModelState.IsValid)
        {
            vm.PhotoUrl ??= student.PhotoUrl;
            return View(vm);
        }

        student.FullName = vm.FullName.Trim();
        student.ParentName = string.IsNullOrWhiteSpace(vm.ParentName) ? null : vm.ParentName.Trim();
        student.Mobile = vm.Mobile.Trim();
        student.ParentMobile = string.IsNullOrWhiteSpace(vm.ParentMobile) ? null : vm.ParentMobile.Trim();
        student.Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim();
        student.Standard = vm.Standard.Trim();
        student.Subject = vm.Subject.Trim();
        student.Address = vm.Address.Trim();
        student.Board = string.IsNullOrWhiteSpace(vm.Board) ? null : vm.Board.Trim();
        student.Medium = string.IsNullOrWhiteSpace(vm.Medium) ? null : vm.Medium.Trim();
        student.RollNumber = string.IsNullOrWhiteSpace(vm.RollNumber) ? null : vm.RollNumber.Trim();
        student.AdmissionNumber = string.IsNullOrWhiteSpace(vm.AdmissionNumber)
            ? student.AdmissionNumber
            : vm.AdmissionNumber.Trim();
        student.DateOfBirth = string.IsNullOrWhiteSpace(vm.DateOfBirth) ? null : vm.DateOfBirth.Trim();
        student.EmergencyContact = string.IsNullOrWhiteSpace(vm.EmergencyContact) ? null : vm.EmergencyContact.Trim();
        student.PreviousSchool = string.IsNullOrWhiteSpace(vm.PreviousSchool) ? null : vm.PreviousSchool.Trim();

        if (photo is { Length: > 0 })
        {
            try
            {
                student.PhotoUrl = await SaveImageUploadAsync(photo, "students", student.PhotoUrl, ct);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                vm.PhotoUrl = student.PhotoUrl;
                return View(vm);
            }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"ID card details updated for {student.FullName}.";
        return RedirectToAction(nameof(StudentIdCard), new { id = student.Id });
    }

    // ═══ ENQUIRIES ════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Enquiries(bool? unread, string? search, CancellationToken ct)
    {
        var all = await portalService.GetEnquiriesAsync(ct);
        if (unread == true) all = all.Where(e => !e.IsRead).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || e.Mobile.Contains(search)).ToList();
        ViewData["Unread"] = unread;
        ViewData["Search"] = search;
        return View(new AdminListPageViewModel<EnquiryAdminViewModel> { Title = "Enquiries", Items = all });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkEnquiryRead(int id, string? reply, CancellationToken ct)
    {
        await portalService.MarkEnquiryReadAsync(id, reply, ct);
        TempData["SuccessMessage"] = "Enquiry marked as read.";
        return RedirectToAction(nameof(Enquiries));
    }

    [HttpGet]
    public async Task<IActionResult> ExportEnquiries(CancellationToken ct)
    {
        var csv = await portalService.ExportEnquiriesCsvAsync(ct);
        return File(csv, "text/csv", $"enquiries_{DateTime.Now:yyyyMMdd}.csv");
    }

    // ═══ COURSES ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Courses(CancellationToken ct) => View(await courseRepo.ListAsync(ct));

    [HttpGet]
    public async Task<IActionResult> CreateCourse(CancellationToken ct)
    {
        var courses = await courseRepo.ListAsync(ct);
        return View("CourseForm", new CourseFormViewModel { DisplayOrder = courses.Count + 1 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseFormViewModel vm, CancellationToken ct)
    {
        NormalizeCourse(vm);
        if (await courseRepo.GetBySlugAsync(vm.Slug, ct) != null)
            ModelState.AddModelError(nameof(vm.Slug), "A course with this slug already exists.");
        if (!ModelState.IsValid) return View("CourseForm", vm);
        await courseRepo.AddAsync(new Core.Entities.Course {
            Slug=vm.Slug, Title=vm.Title, TitleMarathi=vm.TitleMarathi,
            Description=vm.Description, DescriptionMarathi=vm.DescriptionMarathi,
            Standard=vm.Standard, DurationMonths=vm.DurationMonths,
            DisplayOrder=vm.DisplayOrder, IsFeatured=vm.IsFeatured }, ct);
        TempData["SuccessMessage"] = "Course created.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id, CancellationToken ct)
    {
        var c = await courseRepo.GetByIdAsync(id, ct);
        if (c == null) return NotFound();
        return View("CourseForm", new CourseFormViewModel {
            Id=c.Id, Slug=c.Slug, Title=c.Title, TitleMarathi=c.TitleMarathi,
            Description=c.Description, DescriptionMarathi=c.DescriptionMarathi,
            Standard=c.Standard, DurationMonths=c.DurationMonths,
            DisplayOrder=c.DisplayOrder, IsFeatured=c.IsFeatured });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(CourseFormViewModel vm, CancellationToken ct)
    {
        NormalizeCourse(vm);
        var owner = await courseRepo.GetBySlugAsync(vm.Slug, ct);
        if (owner != null && owner.Id != vm.Id)
            ModelState.AddModelError(nameof(vm.Slug), "Slug already exists.");
        if (!ModelState.IsValid) return View("CourseForm", vm);
        var c = await courseRepo.GetByIdAsync(vm.Id, ct);
        if (c == null) return NotFound();
        c.Slug=vm.Slug; c.Title=vm.Title; c.TitleMarathi=vm.TitleMarathi;
        c.Description=vm.Description; c.DescriptionMarathi=vm.DescriptionMarathi;
        c.Standard=vm.Standard; c.DurationMonths=vm.DurationMonths;
        c.DisplayOrder=vm.DisplayOrder; c.IsFeatured=vm.IsFeatured;
        await courseRepo.UpdateAsync(c, ct);
        TempData["SuccessMessage"] = "Course updated.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        await courseRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Course deleted.";
        return RedirectToAction(nameof(Courses));
    }

    // ═══ NOTICES ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Notices(CancellationToken ct) => View(await noticeRepo.GetAllAdminAsync(ct));

    [HttpGet]
    public IActionResult CreateNotice() => View("NoticeForm", new NoticeFormViewModel { PublishedDate = UtcDateTime.StartOfToday() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        vm.PublishedDate = UtcDateTime.EnsureUtc(vm.PublishedDate);
        await noticeRepo.AddAsync(new Core.Entities.Notice {
            Title=vm.Title, TitleMarathi=vm.TitleMarathi, Body=vm.Body,
            BodyMarathi=vm.BodyMarathi, Category=vm.Category, IsPinned=vm.IsPinned,
            IsActive=vm.IsActive, PublishedDate=vm.PublishedDate, CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Notice created.";
        return RedirectToAction(nameof(Notices));
    }

    [HttpGet]
    public async Task<IActionResult> EditNotice(int id, CancellationToken ct)
    {
        var n = await noticeRepo.GetByIdAsync(id, ct);
        if (n == null) return NotFound();
        return View("NoticeForm", new NoticeFormViewModel {
            Id=n.Id, Title=n.Title, TitleMarathi=n.TitleMarathi, Body=n.Body,
            BodyMarathi=n.BodyMarathi, Category=n.Category, IsPinned=n.IsPinned,
            IsActive=n.IsActive, PublishedDate=n.PublishedDate });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        var n = await noticeRepo.GetByIdAsync(vm.Id, ct);
        if (n == null) return NotFound();
        vm.PublishedDate = UtcDateTime.EnsureUtc(vm.PublishedDate);
        n.Title=vm.Title; n.TitleMarathi=vm.TitleMarathi; n.Body=vm.Body;
        n.BodyMarathi=vm.BodyMarathi; n.Category=vm.Category; n.IsPinned=vm.IsPinned;
        n.IsActive=vm.IsActive; n.PublishedDate=vm.PublishedDate;
        await noticeRepo.UpdateAsync(n, ct);
        TempData["SuccessMessage"] = "Notice updated.";
        return RedirectToAction(nameof(Notices));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNotice(int id, CancellationToken ct)
    {
        await noticeRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Notice deleted.";
        return RedirectToAction(nameof(Notices));
    }

    // ═══ RESULTS ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Results(CancellationToken ct) => View(await resultRepo.GetAllAdminAsync(ct));

    [HttpGet]
    public async Task<IActionResult> CreateResult(CancellationToken ct)
    {
        var vm = new TestResultFormViewModel
        {
            TestDate = UtcDateTime.StartOfToday(),
            TotalMarks = 100
        };
        await PopulateResultFormAsync(vm, ct);
        return View("ResultForm", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResult(TestResultFormViewModel vm, CancellationToken ct)
    {
        if (!vm.SelectedStudentId.HasValue)
        {
            ModelState.AddModelError(nameof(vm.SelectedStudentId), "Please select a student.");
        }

        var student = vm.SelectedStudentId.HasValue
            ? await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == vm.SelectedStudentId.Value, ct)
            : null;

        if (vm.SelectedStudentId.HasValue && student is null)
        {
            ModelState.AddModelError(nameof(vm.SelectedStudentId), "The selected student could not be found.");
        }

        if (student is not null)
        {
            vm.StudentName = student.FullName;
            vm.Standard = student.Standard;
        }

        if (!ModelState.IsValid)
        {
            await PopulateResultFormAsync(vm, ct);
            return View("ResultForm", vm);
        }

        vm.TestDate = UtcDateTime.EnsureUtc(vm.TestDate);
        var grade = vm.TotalMarks > 0 ? (int)(vm.Score * 100.0 / vm.TotalMarks) switch {
            >= 90 => "A+", >= 80 => "A", >= 70 => "B+", >= 60 => "B", >= 50 => "C", _ => "D"
        } : null;
        await resultRepo.AddAsync(new Core.Entities.TestResult {
            StudentName=vm.StudentName, Standard=vm.Standard, Subject=vm.Subject,
            Score=vm.Score, TotalMarks=vm.TotalMarks, Rank=vm.Rank, Grade=grade,
            Remarks=vm.Remarks, TestDate=vm.TestDate, TestTitle=vm.TestTitle,
            CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Result added.";
        return RedirectToAction(nameof(Results));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResult(int id, CancellationToken ct)
    {
        await resultRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Result deleted.";
        return RedirectToAction(nameof(Results));
    }

    private async Task PopulateResultFormAsync(TestResultFormViewModel vm, CancellationToken ct)
    {
        vm.AvailableStudents = await db.Students
            .AsNoTracking()
            .OrderBy(s => s.Standard)
            .ThenBy(s => s.FullName)
            .Select(s => new TestResultStudentOptionViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Standard = s.Standard,
                AdmissionNumber = s.AdmissionNumber,
                Mobile = s.Mobile
            })
            .ToListAsync(ct);
    }

    // ═══ STUDY MATERIALS ══════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Materials(CancellationToken ct) => View(await materialRepo.GetAllAdminAsync(ct));

    [HttpGet]
    public IActionResult CreateMaterial() => View("MaterialForm", new StudyMaterialFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMaterial(StudyMaterialFormViewModel vm, CancellationToken ct)
    {
        if (vm.File == null || vm.File.Length == 0)
            ModelState.AddModelError("File", "Please upload a file.");
        if (!ModelState.IsValid) return View("MaterialForm", vm);
        var dir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "materials");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.File!.FileName)}";
        await using var s = System.IO.File.Create(Path.Combine(dir, fileName));
        await vm.File.CopyToAsync(s, ct);
        await materialRepo.AddAsync(new Core.Entities.StudyMaterial {
            Title=vm.Title, TitleMarathi=vm.TitleMarathi, FileUrl=$"/uploads/materials/{fileName}",
            Standard=vm.Standard, Subject=vm.Subject, MaterialType=vm.MaterialType,
            FileSizeBytes=vm.File.Length, IsActive=vm.IsActive, UploadedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Material uploaded.";
        return RedirectToAction(nameof(Materials));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaterial(int id, CancellationToken ct)
    {
        var m = await materialRepo.GetByIdAsync(id, ct);
        if (m != null) {
            var p = Path.Combine(webHostEnvironment.WebRootPath,
                m.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            await materialRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Material deleted.";
        return RedirectToAction(nameof(Materials));
    }

    // ═══ TESTIMONIALS ═════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Testimonials(CancellationToken ct) => View(await testimonialRepo.GetAllAdminAsync(ct));

    [HttpGet]
    public IActionResult CreateTestimonial() => View("TestimonialForm", new TestimonialFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTestimonial(TestimonialFormViewModel vm, CancellationToken ct)
    {
        NormalizeTestimonial(vm);
        if (!ModelState.IsValid) return View("TestimonialForm", vm);
        await testimonialRepo.AddAsync(new Core.Entities.Testimonial {
            Name=vm.Name, Role=vm.Role, Quote=vm.Quote, QuoteMarathi=vm.QuoteMarathi,
            Rating=vm.Rating, IsApproved=vm.IsApproved, IsFeatured=vm.IsFeatured,
            CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Testimonial created.";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpGet]
    public async Task<IActionResult> EditTestimonial(int id, CancellationToken ct)
    {
        var t = await testimonialRepo.GetByIdAsync(id, ct);
        if (t == null) return NotFound();
        return View("TestimonialForm", new TestimonialFormViewModel {
            Id=t.Id, Name=t.Name, Role=t.Role, Quote=t.Quote, QuoteMarathi=t.QuoteMarathi,
            Rating=t.Rating, IsApproved=t.IsApproved, IsFeatured=t.IsFeatured });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTestimonial(TestimonialFormViewModel vm, CancellationToken ct)
    {
        NormalizeTestimonial(vm);
        if (!ModelState.IsValid) return View("TestimonialForm", vm);
        var t = await testimonialRepo.GetByIdAsync(vm.Id, ct);
        if (t == null) return NotFound();
        t.Name=vm.Name; t.Role=vm.Role; t.Quote=vm.Quote; t.QuoteMarathi=vm.QuoteMarathi;
        t.Rating=vm.Rating; t.IsApproved=vm.IsApproved; t.IsFeatured=vm.IsFeatured;
        await testimonialRepo.UpdateAsync(t, ct);
        TempData["SuccessMessage"] = "Testimonial updated.";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTestimonial(int id, bool featured, CancellationToken ct)
    {
        var t = await testimonialRepo.GetByIdAsync(id, ct);
        if (t == null) return NotFound();
        t.IsApproved = true;
        if (featured) t.IsFeatured = true;
        await testimonialRepo.UpdateAsync(t, ct);
        TempData["SuccessMessage"] = featured ? "Testimonial approved and featured." : "Testimonial approved.";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestimonial(int id, CancellationToken ct)
    {
        await testimonialRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Testimonial deleted.";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpGet]
    public async Task<IActionResult> HomePageContent(CancellationToken ct)
    {
        var (settings, usedFallback) = await GetHomePageSectionSettingsAsync(ct);
        if (usedFallback)
            ViewBag.ContentSettingsWarning = "Homepage content settings are temporarily using safe defaults because the production database schema is behind the deployed code.";

        return View(new HomePageContentAdminViewModel
        {
            CurrentHeroBannerImageUrl = settings.HeroBannerImageUrl,
            HeroBannerAltText = settings.HeroBannerAltText,
            ShowTrendingBanner = settings.ShowTrendingBanner,
            CurrentTrendingImageUrl = settings.TrendingImageUrl,
            TrendingEyebrow = settings.TrendingEyebrow,
            TrendingEyebrowMarathi = settings.TrendingEyebrowMarathi,
            TrendingTitle = settings.TrendingTitle,
            TrendingTitleMarathi = settings.TrendingTitleMarathi,
            TrendingDescription = settings.TrendingDescription,
            TrendingDescriptionMarathi = settings.TrendingDescriptionMarathi,
            TrendingAltText = settings.TrendingAltText,
            TrendingLinkText = settings.TrendingLinkText,
            TrendingLinkTextMarathi = settings.TrendingLinkTextMarathi,
            TrendingLinkUrl = settings.TrendingLinkUrl,
            ShowStatisticsSection = settings.ShowStatisticsSection,
            Stat1Value = settings.Stat1Value,
            Stat1Label = settings.Stat1Label,
            Stat1LabelMarathi = settings.Stat1LabelMarathi,
            Stat2Value = settings.Stat2Value,
            Stat2Label = settings.Stat2Label,
            Stat2LabelMarathi = settings.Stat2LabelMarathi,
            Stat3Value = settings.Stat3Value,
            Stat3Label = settings.Stat3Label,
            Stat3LabelMarathi = settings.Stat3LabelMarathi,
            Stat4Value = settings.Stat4Value,
            Stat4Label = settings.Stat4Label,
            Stat4LabelMarathi = settings.Stat4LabelMarathi,
            ShowTestimonialsSection = settings.ShowTestimonialsSection,
            TestimonialsEyebrow = settings.TestimonialsEyebrow,
            TestimonialsEyebrowMarathi = settings.TestimonialsEyebrowMarathi,
            TestimonialsTitle = settings.TestimonialsTitle,
            TestimonialsTitleMarathi = settings.TestimonialsTitleMarathi
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HomePageContent(HomePageContentAdminViewModel vm, IFormFile? heroBannerImage, IFormFile? trendingBannerImage, CancellationToken ct)
    {
        NormalizeHomePageContent(vm);
        var (settings, usedFallback) = await GetHomePageSectionSettingsAsync(ct);
        vm.CurrentHeroBannerImageUrl = settings.HeroBannerImageUrl;
        vm.CurrentTrendingImageUrl = settings.TrendingImageUrl;
        if (usedFallback)
            ModelState.AddModelError(string.Empty, "Homepage content cannot be saved yet because the production database schema is behind the deployed code.");
        if (!ModelState.IsValid) return View(vm);

        if (heroBannerImage is { Length: > 0 })
        {
            try
            {
                settings.HeroBannerImageUrl = await SaveImageUploadAsync(heroBannerImage, "homepage", settings.HeroBannerImageUrl, ct);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        if (trendingBannerImage is { Length: > 0 })
        {
            try
            {
                settings.TrendingImageUrl = await SaveImageUploadAsync(trendingBannerImage, "homepage", settings.TrendingImageUrl, ct);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        settings.HeroBannerAltText = vm.HeroBannerAltText;
        settings.ShowTrendingBanner = vm.ShowTrendingBanner;
        settings.TrendingEyebrow = vm.TrendingEyebrow;
        settings.TrendingEyebrowMarathi = vm.TrendingEyebrowMarathi;
        settings.TrendingTitle = vm.TrendingTitle;
        settings.TrendingTitleMarathi = vm.TrendingTitleMarathi;
        settings.TrendingDescription = vm.TrendingDescription;
        settings.TrendingDescriptionMarathi = vm.TrendingDescriptionMarathi;
        settings.TrendingAltText = vm.TrendingAltText;
        settings.TrendingLinkText = vm.TrendingLinkText;
        settings.TrendingLinkTextMarathi = vm.TrendingLinkTextMarathi;
        settings.TrendingLinkUrl = vm.TrendingLinkUrl;
        settings.ShowStatisticsSection = vm.ShowStatisticsSection;
        settings.Stat1Value = vm.Stat1Value;
        settings.Stat1Label = vm.Stat1Label;
        settings.Stat1LabelMarathi = vm.Stat1LabelMarathi;
        settings.Stat2Value = vm.Stat2Value;
        settings.Stat2Label = vm.Stat2Label;
        settings.Stat2LabelMarathi = vm.Stat2LabelMarathi;
        settings.Stat3Value = vm.Stat3Value;
        settings.Stat3Label = vm.Stat3Label;
        settings.Stat3LabelMarathi = vm.Stat3LabelMarathi;
        settings.Stat4Value = vm.Stat4Value;
        settings.Stat4Label = vm.Stat4Label;
        settings.Stat4LabelMarathi = vm.Stat4LabelMarathi;
        settings.ShowTestimonialsSection = vm.ShowTestimonialsSection;
        settings.TestimonialsEyebrow = vm.TestimonialsEyebrow;
        settings.TestimonialsEyebrowMarathi = vm.TestimonialsEyebrowMarathi;
        settings.TestimonialsTitle = vm.TestimonialsTitle;
        settings.TestimonialsTitleMarathi = vm.TestimonialsTitleMarathi;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (db.Database.IsSqlServer())
        {
            logger.LogWarning(ex, "Homepage content save failed because the SQL Server schema is behind the deployed code.");
            ModelState.AddModelError(string.Empty, "Homepage content could not be saved because the production database schema is not fully updated yet.");
            return View(vm);
        }

        TempData["SuccessMessage"] = "Homepage hero, trending banner, stats, and testimonials settings updated.";
        return RedirectToAction(nameof(HomePageContent));
    }

    [HttpGet]
    public async Task<IActionResult> AboutPageContent(CancellationToken ct)
    {
        var (settings, usedFallback) = await GetAboutPageSectionSettingsAsync(ct);
        if (usedFallback)
            ViewBag.ContentSettingsWarning = "About page content settings are temporarily using safe defaults because the production database schema is behind the deployed code.";

        return View(new AboutPageContentAdminViewModel
        {
            ShowStatisticsSection = settings.ShowStatisticsSection,
            Stat1Value = settings.Stat1Value,
            Stat1Label = settings.Stat1Label,
            Stat1LabelMarathi = settings.Stat1LabelMarathi,
            Stat2Value = settings.Stat2Value,
            Stat2Label = settings.Stat2Label,
            Stat2LabelMarathi = settings.Stat2LabelMarathi,
            Stat3Value = settings.Stat3Value,
            Stat3Label = settings.Stat3Label,
            Stat3LabelMarathi = settings.Stat3LabelMarathi,
            Stat4Value = settings.Stat4Value,
            Stat4Label = settings.Stat4Label,
            Stat4LabelMarathi = settings.Stat4LabelMarathi,
            Address = settings.Address,
            AddressMarathi = settings.AddressMarathi,
            MapEmbedUrl = settings.MapEmbedUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AboutPageContent(AboutPageContentAdminViewModel vm, CancellationToken ct)
    {
        NormalizeAboutPageContent(vm);
        if (!ModelState.IsValid) return View(vm);

        var (settings, usedFallback) = await GetAboutPageSectionSettingsAsync(ct);
        if (usedFallback)
        {
            ModelState.AddModelError(string.Empty, "About page content cannot be saved yet because the production database schema is behind the deployed code.");
            return View(vm);
        }

        settings.ShowStatisticsSection = vm.ShowStatisticsSection;
        settings.Stat1Value = vm.Stat1Value;
        settings.Stat1Label = vm.Stat1Label;
        settings.Stat1LabelMarathi = vm.Stat1LabelMarathi;
        settings.Stat2Value = vm.Stat2Value;
        settings.Stat2Label = vm.Stat2Label;
        settings.Stat2LabelMarathi = vm.Stat2LabelMarathi;
        settings.Stat3Value = vm.Stat3Value;
        settings.Stat3Label = vm.Stat3Label;
        settings.Stat3LabelMarathi = vm.Stat3LabelMarathi;
        settings.Stat4Value = vm.Stat4Value;
        settings.Stat4Label = vm.Stat4Label;
        settings.Stat4LabelMarathi = vm.Stat4LabelMarathi;
        settings.Address = vm.Address;
        settings.AddressMarathi = vm.AddressMarathi;
        settings.MapEmbedUrl = vm.MapEmbedUrl;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (db.Database.IsSqlServer())
        {
            logger.LogWarning(ex, "About page content save failed because the SQL Server schema is behind the deployed code.");
            ModelState.AddModelError(string.Empty, "About page content could not be saved because the production database schema is not fully updated yet.");
            return View(vm);
        }

        TempData["SuccessMessage"] = "About page settings updated.";
        return RedirectToAction(nameof(AboutPageContent));
    }

    // ═══ GALLERY ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Gallery(CancellationToken ct)
        => View("AdminGallery", await galleryRepo.GetAllAdminAsync(ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadGalleryItem(
        string title, string category, string? caption, IFormFile image, CancellationToken ct)
    {
        if (image != null && image.Length > 0)
        {
            var dir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(dir);
            var fn = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            await using var s = System.IO.File.Create(Path.Combine(dir, fn));
            await image.CopyToAsync(s, ct);
            await galleryRepo.AddAsync(new Core.Entities.GalleryItem {
                Title=title, ImageUrl=$"/uploads/gallery/{fn}", Caption=caption,
                Category=string.IsNullOrWhiteSpace(category) ? "General" : category,
                DisplayOrder=99, IsActive=true, CreatedDate=DateTime.UtcNow }, ct);
        }
        TempData["SuccessMessage"] = "Image uploaded.";
        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGalleryItem(int id, CancellationToken ct)
    {
        var items = await galleryRepo.GetAllAdminAsync(ct);
        var item = items.FirstOrDefault(x => x.Id == id);
        if (item != null) {
            var p = Path.Combine(webHostEnvironment.WebRootPath,
                item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            await galleryRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Gallery item deleted.";
        return RedirectToAction(nameof(Gallery));
    }

    // ═══ AUDIT LOG ════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> AuditLog(int page = 1, CancellationToken ct = default)
    {
        const int pageSize = 50;
        var total = await db.AuditLogs.CountAsync(ct);
        var items = await db.AuditLogs
            .OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    [HttpGet]
    public IActionResult ChangePassword()
        => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        if (!TryGetCurrentUserId(out var userId))
            return Forbid();

        var result = await portalUsers.ChangePasswordAsync(userId, vm.CurrentPassword, vm.NewPassword, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(vm);
        }

        TempData["SuccessMessage"] = "Your password has been updated.";
        return RedirectToAction(nameof(ChangePassword));
    }

    [HttpGet]
    public async Task<IActionResult> PortalAccounts(CancellationToken ct)
    {
        try
        {
            var users = await db.AppUsers
                .Where(u => u.Role == "Teacher" || u.Role == "Parent")
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync(ct);

            var teachers = (await teacherRepo.GetAllAsync(ct))
                .ToDictionary(t => t.Id, t => t.FullName);
            var students = await db.Students
                .ToDictionaryAsync(s => s.Id, s => s.FullName, ct);

            var model = users.Select(u => new PortalAccountAdminViewModel
            {
                UserId = u.Id,
                Username = u.Username,
                Role = u.Role,
                FullName = u.FullName ?? u.Username,
                Mobile = u.Mobile,
                LinkedTo = u.TeacherId.HasValue && teachers.TryGetValue(u.TeacherId.Value, out var teacherName)
                    ? teacherName
                    : u.StudentId.HasValue && students.TryGetValue(u.StudentId.Value, out var studentName)
                        ? studentName
                        : "Not linked",
                IsActive = u.IsActive
            }).ToList();

            return View(model);
        }
        catch
        {
            ViewBag.PageLoadWarning = "Portal account data is temporarily unavailable. The page is running in safe mode.";
            return View(Array.Empty<PortalAccountAdminViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ResetPortalPassword(int id, CancellationToken ct)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Id == id && (u.Role == "Teacher" || u.Role == "Parent"), ct);
        if (user is null) return NotFound();

        return View(new AdminResetPasswordViewModel
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            FullName = user.FullName ?? user.Username
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPortalPassword(AdminResetPasswordViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Id == vm.UserId && (u.Role == "Teacher" || u.Role == "Parent"), ct);
        if (user is null) return NotFound();

        var result = await portalUsers.SetPasswordAsync(user.Id, vm.NewPassword, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(vm);
        }

        TempData["SuccessMessage"] = $"{user.Role} password updated for {user.FullName ?? user.Username}.";
        return RedirectToAction(nameof(PortalAccounts));
    }

    // ═══ HELPERS ══════════════════════════════════════════════════════════════
    private bool TryGetCurrentUserId(out int userId)
        => int.TryParse(User.FindFirst("UserId")?.Value, out userId) && userId > 0;

    private static void NormalizeCourse(CourseFormViewModel vm)
    {
        vm.Title             = vm.Title.Trim();
        vm.TitleMarathi      = string.IsNullOrWhiteSpace(vm.TitleMarathi) ? vm.Title : vm.TitleMarathi.Trim();
        vm.Description       = vm.Description.Trim();
        vm.DescriptionMarathi = string.IsNullOrWhiteSpace(vm.DescriptionMarathi) ? vm.Description : vm.DescriptionMarathi.Trim();
        vm.Standard          = vm.Standard.Trim();
        vm.Slug              = string.IsNullOrWhiteSpace(vm.Slug)
            ? vm.Title.Trim().ToLowerInvariant().Replace(" ", "-")
            : vm.Slug.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private static void NormalizeTestimonial(TestimonialFormViewModel vm)
    {
        vm.Name        = vm.Name.Trim();
        vm.Role        = string.IsNullOrWhiteSpace(vm.Role) ? "Parent / Student" : vm.Role.Trim();
        vm.Quote       = vm.Quote.Trim();
        vm.QuoteMarathi = string.IsNullOrWhiteSpace(vm.QuoteMarathi) ? null : vm.QuoteMarathi.Trim();
        vm.Rating      = Math.Clamp(vm.Rating, 1, 5);
    }

    private static void NormalizeHomePageContent(HomePageContentAdminViewModel vm)
    {
        vm.HeroBannerAltText = vm.HeroBannerAltText.Trim();
        vm.TrendingEyebrow = vm.TrendingEyebrow.Trim();
        vm.TrendingEyebrowMarathi = vm.TrendingEyebrowMarathi.Trim();
        vm.TrendingTitle = vm.TrendingTitle.Trim();
        vm.TrendingTitleMarathi = vm.TrendingTitleMarathi.Trim();
        vm.TrendingDescription = vm.TrendingDescription.Trim();
        vm.TrendingDescriptionMarathi = vm.TrendingDescriptionMarathi.Trim();
        vm.TrendingAltText = vm.TrendingAltText.Trim();
        vm.TrendingLinkText = vm.TrendingLinkText.Trim();
        vm.TrendingLinkTextMarathi = vm.TrendingLinkTextMarathi.Trim();
        vm.TrendingLinkUrl = vm.TrendingLinkUrl.Trim();
        vm.Stat1Value = vm.Stat1Value.Trim();
        vm.Stat1Label = vm.Stat1Label.Trim();
        vm.Stat1LabelMarathi = vm.Stat1LabelMarathi.Trim();
        vm.Stat2Value = vm.Stat2Value.Trim();
        vm.Stat2Label = vm.Stat2Label.Trim();
        vm.Stat2LabelMarathi = vm.Stat2LabelMarathi.Trim();
        vm.Stat3Value = vm.Stat3Value.Trim();
        vm.Stat3Label = vm.Stat3Label.Trim();
        vm.Stat3LabelMarathi = vm.Stat3LabelMarathi.Trim();
        vm.Stat4Value = vm.Stat4Value.Trim();
        vm.Stat4Label = vm.Stat4Label.Trim();
        vm.Stat4LabelMarathi = vm.Stat4LabelMarathi.Trim();
        vm.TestimonialsEyebrow = vm.TestimonialsEyebrow.Trim();
        vm.TestimonialsEyebrowMarathi = vm.TestimonialsEyebrowMarathi.Trim();
        vm.TestimonialsTitle = vm.TestimonialsTitle.Trim();
        vm.TestimonialsTitleMarathi = vm.TestimonialsTitleMarathi.Trim();
    }

    private static void NormalizeAboutPageContent(AboutPageContentAdminViewModel vm)
    {
        vm.Stat1Value = vm.Stat1Value.Trim();
        vm.Stat1Label = vm.Stat1Label.Trim();
        vm.Stat1LabelMarathi = vm.Stat1LabelMarathi.Trim();
        vm.Stat2Value = vm.Stat2Value.Trim();
        vm.Stat2Label = vm.Stat2Label.Trim();
        vm.Stat2LabelMarathi = vm.Stat2LabelMarathi.Trim();
        vm.Stat3Value = vm.Stat3Value.Trim();
        vm.Stat3Label = vm.Stat3Label.Trim();
        vm.Stat3LabelMarathi = vm.Stat3LabelMarathi.Trim();
        vm.Stat4Value = vm.Stat4Value.Trim();
        vm.Stat4Label = vm.Stat4Label.Trim();
        vm.Stat4LabelMarathi = vm.Stat4LabelMarathi.Trim();
        vm.Address = vm.Address.Trim();
        vm.AddressMarathi = vm.AddressMarathi.Trim();
        vm.MapEmbedUrl = vm.MapEmbedUrl.Trim();
    }

    private async Task<(Core.Entities.HomePageSectionSettings Settings, bool UsedFallback)> GetHomePageSectionSettingsAsync(CancellationToken ct)
    {
        try
        {
            var settings = await db.HomePageSectionSettings.FirstOrDefaultAsync(ct);
            if (settings is not null) return (settings, false);

            settings = new Core.Entities.HomePageSectionSettings();
            db.HomePageSectionSettings.Add(settings);
            await db.SaveChangesAsync(ct);
            return (settings, false);
        }
        catch (Exception ex) when (db.Database.IsSqlServer())
        {
            logger.LogWarning(ex, "Homepage content settings are unavailable because the SQL Server schema is behind the deployed code.");
            return (new Core.Entities.HomePageSectionSettings(), true);
        }
    }

    private async Task<(Core.Entities.AboutPageSectionSettings Settings, bool UsedFallback)> GetAboutPageSectionSettingsAsync(CancellationToken ct)
    {
        try
        {
            var settings = await db.AboutPageSectionSettings.FirstOrDefaultAsync(ct);
            if (settings is not null) return (settings, false);

            settings = new Core.Entities.AboutPageSectionSettings();
            db.AboutPageSectionSettings.Add(settings);
            await db.SaveChangesAsync(ct);
            return (settings, false);
        }
        catch (Exception ex) when (db.Database.IsSqlServer())
        {
            logger.LogWarning(ex, "About page content settings are unavailable because the SQL Server schema is behind the deployed code.");
            return (new Core.Entities.AboutPageSectionSettings(), true);
        }
    }

    private async Task<string> SaveImageUploadAsync(IFormFile file, string folderName, string? existingUrl, CancellationToken ct)
    {
        ValidateImageUpload(file);
        var extension = Path.GetExtension(file.FileName);

        var folder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absolutePath = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(absolutePath);
        await file.CopyToAsync(stream, ct);

        DeleteLocalUpload(existingUrl);
        return $"/uploads/{folderName}/{fileName}";
    }

    private void DeleteLocalUpload(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        var path = Path.Combine(webHostEnvironment.WebRootPath, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    private static void ValidateImageUpload(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Only JPG, PNG, WEBP, and GIF images are supported.");
    }

    private static bool EnsureAdmissionNumber(Core.Entities.Student student)
    {
        if (!string.IsNullOrWhiteSpace(student.AdmissionNumber))
            return false;

        student.AdmissionNumber = $"SK{student.CreatedDate:yyyy}{student.Id:D4}";
        return true;
    }

    private static StudentIdCardAdminViewModel MapStudentIdCard(Core.Entities.Student student)
        => new()
        {
            Id = student.Id,
            FullName = student.FullName,
            ParentName = student.ParentName,
            Mobile = student.Mobile,
            ParentMobile = student.ParentMobile,
            Email = student.Email,
            Standard = student.Standard,
            Subject = student.Subject,
            Address = student.Address,
            Board = student.Board,
            Medium = student.Medium,
            Status = student.Status,
            AdmissionNumber = student.AdmissionNumber,
            RollNumber = student.RollNumber,
            DateOfBirth = student.DateOfBirth,
            PhotoUrl = student.PhotoUrl,
            EmergencyContact = student.EmergencyContact,
            PreviousSchool = student.PreviousSchool,
            CreatedDate = student.CreatedDate
        };
}
