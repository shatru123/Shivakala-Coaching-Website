using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

public sealed class AdminController(
    IAdminAuthenticationService authService,
    IAdminPortalService portalService,
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
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(AdminLoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Login", model);
        if (!authService.ValidateCredentials(model.Username, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View("Login", model);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username.Trim()),
            new(ClaimTypes.Role, "Admin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        logger.LogInformation("Admin logged in: {User}", model.Username);
        return !string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl) : RedirectToAction(nameof(Index));
    }

    // GET logout — safe to call from a plain <a> link in sidebar
    [HttpGet, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        logger.LogInformation("Admin signed out.");
        return RedirectToAction(nameof(Login));
    }

    // ═══ DASHBOARD ════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dashboard = await portalService.GetDashboardAsync(ct);

        // Live stats injected into ViewBag
        ViewBag.TotalStudents    = await db.Students.CountAsync(ct);
        ViewBag.TotalTeachers    = await db.Teachers.CountAsync(s => s.IsActive, ct);
        ViewBag.TotalBatches     = await db.Batches.CountAsync(b => b.IsActive, ct);
        ViewBag.TotalEnquiries   = await db.Enquiries.CountAsync(e => !e.IsRead, ct);
        ViewBag.FeeThisMonth     = await db.FeePayments
            .Where(f => f.Month == DateTime.UtcNow.ToString("yyyy-MM") && f.Status == "Paid")
            .SumAsync(f => (decimal?)f.PaidAmount, ct) ?? 0;
        ViewBag.PendingFees      = await db.FeePayments
            .Where(f => f.Status == "Pending")
            .SumAsync(f => (decimal?)(f.Amount - f.PaidAmount), ct) ?? 0;
        ViewBag.UpcomingExams    = await db.Exams
            .Where(e => e.ExamDate >= DateTime.Today && !e.IsPublished)
            .CountAsync(ct);
        ViewBag.PendingHomework  = await db.Homeworks
            .Where(h => h.IsActive && h.DueDate >= DateTime.Today)
            .CountAsync(ct);

        return View(dashboard);
    }

    // ═══ REGISTRATIONS ════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Registrations(string? status, string? search, CancellationToken ct)
    {
        var all = await portalService.GetRegistrationsAsync(ct);
        if (!string.IsNullOrWhiteSpace(status)) all = all.Where(r => r.Status == status).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(r => r.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || r.Mobile.Contains(search)).ToList();
        ViewData["Status"] = status; ViewData["Search"] = search;
        return View(new AdminListPageViewModel<StudentAdminViewModel> { Title = "Registrations", Items = all });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudentStatus(int id, string status, string? notes, CancellationToken ct)
    {
        await portalService.UpdateStudentStatusAsync(id, status, notes, ct);
        TempData["SuccessMessage"] = $"Student #{id} status updated to {status}.";
        return RedirectToAction(nameof(Registrations));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> ExportRegistrations(CancellationToken ct)
    {
        var csv = await portalService.ExportRegistrationsCsvAsync(ct);
        return File(csv, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd}.csv");
    }

    // ═══ ENQUIRIES ════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Enquiries(bool? unread, string? search, CancellationToken ct)
    {
        var all = await portalService.GetEnquiriesAsync(ct);
        if (unread == true) all = all.Where(e => !e.IsRead).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || e.Mobile.Contains(search)).ToList();
        ViewData["Unread"] = unread; ViewData["Search"] = search;
        return View(new AdminListPageViewModel<EnquiryAdminViewModel> { Title = "Enquiries", Items = all });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkEnquiryRead(int id, string? reply, CancellationToken ct)
    {
        await portalService.MarkEnquiryReadAsync(id, reply, ct);
        TempData["SuccessMessage"] = "Enquiry marked as read.";
        return RedirectToAction(nameof(Enquiries));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> ExportEnquiries(CancellationToken ct)
    {
        var csv = await portalService.ExportEnquiriesCsvAsync(ct);
        return File(csv, "text/csv", $"enquiries_{DateTime.Now:yyyyMMdd}.csv");
    }

    // ═══ COURSES ══════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Courses(CancellationToken ct) => View(await courseRepo.ListAsync(ct));

    [Authorize, HttpGet]
    public async Task<IActionResult> CreateCourse(CancellationToken ct)
    {
        var courses = await courseRepo.ListAsync(ct);
        return View("CourseForm", new CourseFormViewModel { DisplayOrder = courses.Count + 1 });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseFormViewModel vm, CancellationToken ct)
    {
        NormalizeCourse(vm);
        if (await courseRepo.GetBySlugAsync(vm.Slug, ct) != null)
            ModelState.AddModelError(nameof(vm.Slug), "A course with this slug already exists.");
        if (!ModelState.IsValid) return View("CourseForm", vm);
        await courseRepo.AddAsync(new Core.Entities.Course {
            Slug=vm.Slug,Title=vm.Title,TitleMarathi=vm.TitleMarathi,
            Description=vm.Description,DescriptionMarathi=vm.DescriptionMarathi,
            Standard=vm.Standard,DurationMonths=vm.DurationMonths,
            DisplayOrder=vm.DisplayOrder,IsFeatured=vm.IsFeatured }, ct);
        TempData["SuccessMessage"] = "Course created.";
        return RedirectToAction(nameof(Courses));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditCourse(int id, CancellationToken ct)
    {
        var c = await courseRepo.GetByIdAsync(id, ct);
        if (c == null) return NotFound();
        return View("CourseForm", new CourseFormViewModel {
            Id=c.Id,Slug=c.Slug,Title=c.Title,TitleMarathi=c.TitleMarathi,
            Description=c.Description,DescriptionMarathi=c.DescriptionMarathi,
            Standard=c.Standard,DurationMonths=c.DurationMonths,
            DisplayOrder=c.DisplayOrder,IsFeatured=c.IsFeatured });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(CourseFormViewModel vm, CancellationToken ct)
    {
        NormalizeCourse(vm);
        var owner = await courseRepo.GetBySlugAsync(vm.Slug, ct);
        if (owner != null && owner.Id != vm.Id)
            ModelState.AddModelError(nameof(vm.Slug), "Slug already exists.");
        if (!ModelState.IsValid) return View("CourseForm", vm);
        var c = await courseRepo.GetByIdAsync(vm.Id, ct);
        if (c == null) return NotFound();
        c.Slug=vm.Slug;c.Title=vm.Title;c.TitleMarathi=vm.TitleMarathi;
        c.Description=vm.Description;c.DescriptionMarathi=vm.DescriptionMarathi;
        c.Standard=vm.Standard;c.DurationMonths=vm.DurationMonths;
        c.DisplayOrder=vm.DisplayOrder;c.IsFeatured=vm.IsFeatured;
        await courseRepo.UpdateAsync(c, ct);
        TempData["SuccessMessage"] = "Course updated.";
        return RedirectToAction(nameof(Courses));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        await courseRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Course deleted.";
        return RedirectToAction(nameof(Courses));
    }

    // ═══ NOTICES ══════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Notices(CancellationToken ct) => View(await noticeRepo.GetAllAdminAsync(ct));

    [Authorize, HttpGet]
    public IActionResult CreateNotice() => View("NoticeForm", new NoticeFormViewModel { PublishedDate = DateTime.Today });

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        await noticeRepo.AddAsync(new Core.Entities.Notice {
            Title=vm.Title,TitleMarathi=vm.TitleMarathi,Body=vm.Body,
            BodyMarathi=vm.BodyMarathi,Category=vm.Category,IsPinned=vm.IsPinned,
            IsActive=vm.IsActive,PublishedDate=vm.PublishedDate,CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Notice created.";
        return RedirectToAction(nameof(Notices));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditNotice(int id, CancellationToken ct)
    {
        var n = await noticeRepo.GetByIdAsync(id, ct);
        if (n == null) return NotFound();
        return View("NoticeForm", new NoticeFormViewModel {
            Id=n.Id,Title=n.Title,TitleMarathi=n.TitleMarathi,Body=n.Body,
            BodyMarathi=n.BodyMarathi,Category=n.Category,IsPinned=n.IsPinned,
            IsActive=n.IsActive,PublishedDate=n.PublishedDate });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        var n = await noticeRepo.GetByIdAsync(vm.Id, ct);
        if (n == null) return NotFound();
        n.Title=vm.Title;n.TitleMarathi=vm.TitleMarathi;n.Body=vm.Body;
        n.BodyMarathi=vm.BodyMarathi;n.Category=vm.Category;n.IsPinned=vm.IsPinned;
        n.IsActive=vm.IsActive;n.PublishedDate=vm.PublishedDate;
        await noticeRepo.UpdateAsync(n, ct);
        TempData["SuccessMessage"] = "Notice updated.";
        return RedirectToAction(nameof(Notices));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNotice(int id, CancellationToken ct)
    {
        await noticeRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Notice deleted.";
        return RedirectToAction(nameof(Notices));
    }

    // ═══ RESULTS ══════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Results(CancellationToken ct) => View(await resultRepo.GetAllAdminAsync(ct));

    [Authorize, HttpGet]
    public IActionResult CreateResult() => View("ResultForm", new TestResultFormViewModel { TestDate = DateTime.Today, TotalMarks = 100 });

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResult(TestResultFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("ResultForm", vm);
        var grade = vm.TotalMarks > 0 ? (int)(vm.Score * 100.0 / vm.TotalMarks) switch {
            >= 90 => "A+", >= 80 => "A", >= 70 => "B+", >= 60 => "B", >= 50 => "C", _ => "D"
        } : null;
        await resultRepo.AddAsync(new Core.Entities.TestResult {
            StudentName=vm.StudentName,Standard=vm.Standard,Subject=vm.Subject,
            Score=vm.Score,TotalMarks=vm.TotalMarks,Rank=vm.Rank,Grade=grade,
            Remarks=vm.Remarks,TestDate=vm.TestDate,TestTitle=vm.TestTitle,CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Result added.";
        return RedirectToAction(nameof(Results));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResult(int id, CancellationToken ct)
    {
        await resultRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Result deleted.";
        return RedirectToAction(nameof(Results));
    }

    // ═══ STUDY MATERIALS ══════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Materials(CancellationToken ct) => View(await materialRepo.GetAllAdminAsync(ct));

    [Authorize, HttpGet]
    public IActionResult CreateMaterial() => View("MaterialForm", new StudyMaterialFormViewModel());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMaterial(StudyMaterialFormViewModel vm, CancellationToken ct)
    {
        if (vm.File == null || vm.File.Length == 0) ModelState.AddModelError("File", "Please upload a file.");
        if (!ModelState.IsValid) return View("MaterialForm", vm);
        var dir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "materials");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.File!.FileName)}";
        await using var s = System.IO.File.Create(Path.Combine(dir, fileName));
        await vm.File.CopyToAsync(s, ct);
        await materialRepo.AddAsync(new Core.Entities.StudyMaterial {
            Title=vm.Title,TitleMarathi=vm.TitleMarathi,FileUrl=$"/uploads/materials/{fileName}",
            Standard=vm.Standard,Subject=vm.Subject,MaterialType=vm.MaterialType,
            FileSizeBytes=vm.File.Length,IsActive=vm.IsActive,UploadedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Material uploaded.";
        return RedirectToAction(nameof(Materials));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaterial(int id, CancellationToken ct)
    {
        var m = await materialRepo.GetByIdAsync(id, ct);
        if (m != null) {
            var p = Path.Combine(webHostEnvironment.WebRootPath, m.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            await materialRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Material deleted.";
        return RedirectToAction(nameof(Materials));
    }

    // ═══ TESTIMONIALS ═════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Testimonials(CancellationToken ct) => View(await testimonialRepo.GetAllAdminAsync(ct));
    [Authorize, HttpGet]
    public IActionResult CreateTestimonial() => View("TestimonialForm", new TestimonialFormViewModel());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTestimonial(TestimonialFormViewModel vm, CancellationToken ct)
    {
        NormalizeTestimonial(vm);
        if (!ModelState.IsValid) return View("TestimonialForm", vm);
        await testimonialRepo.AddAsync(new Core.Entities.Testimonial {
            Name=vm.Name,Role=vm.Role,Quote=vm.Quote,QuoteMarathi=vm.QuoteMarathi,
            Rating=vm.Rating,IsApproved=vm.IsApproved,IsFeatured=vm.IsFeatured,CreatedDate=DateTime.UtcNow }, ct);
        TempData["SuccessMessage"] = "Testimonial created.";
        return RedirectToAction(nameof(Testimonials));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditTestimonial(int id, CancellationToken ct)
    {
        var t = await testimonialRepo.GetByIdAsync(id, ct);
        if (t == null) return NotFound();
        return View("TestimonialForm", new TestimonialFormViewModel {
            Id=t.Id,Name=t.Name,Role=t.Role,Quote=t.Quote,QuoteMarathi=t.QuoteMarathi,
            Rating=t.Rating,IsApproved=t.IsApproved,IsFeatured=t.IsFeatured });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTestimonial(TestimonialFormViewModel vm, CancellationToken ct)
    {
        NormalizeTestimonial(vm);
        if (!ModelState.IsValid) return View("TestimonialForm", vm);
        var t = await testimonialRepo.GetByIdAsync(vm.Id, ct);
        if (t == null) return NotFound();
        t.Name=vm.Name;t.Role=vm.Role;t.Quote=vm.Quote;t.QuoteMarathi=vm.QuoteMarathi;
        t.Rating=vm.Rating;t.IsApproved=vm.IsApproved;t.IsFeatured=vm.IsFeatured;
        await testimonialRepo.UpdateAsync(t, ct);
        TempData["SuccessMessage"] = "Testimonial updated.";
        return RedirectToAction(nameof(Testimonials));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestimonial(int id, CancellationToken ct)
    {
        await testimonialRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Testimonial deleted.";
        return RedirectToAction(nameof(Testimonials));
    }

    // ═══ GALLERY ══════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> Gallery(CancellationToken ct) => View("AdminGallery", await galleryRepo.GetAllAdminAsync(ct));

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadGalleryItem(string title, string category, string? caption, IFormFile image, CancellationToken ct)
    {
        if (image != null && image.Length > 0)
        {
            var dir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(dir);
            var fn = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            await using var s = System.IO.File.Create(Path.Combine(dir, fn));
            await image.CopyToAsync(s, ct);
            await galleryRepo.AddAsync(new Core.Entities.GalleryItem {
                Title=title,ImageUrl=$"/uploads/gallery/{fn}",Caption=caption,
                Category=string.IsNullOrWhiteSpace(category)?"General":category,
                DisplayOrder=99,IsActive=true,CreatedDate=DateTime.UtcNow }, ct);
        }
        TempData["SuccessMessage"] = "Image uploaded.";
        return RedirectToAction(nameof(Gallery));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGalleryItem(int id, CancellationToken ct)
    {
        var items = await galleryRepo.GetAllAdminAsync(ct);
        var item = items.FirstOrDefault(x => x.Id == id);
        if (item != null) {
            var p = Path.Combine(webHostEnvironment.WebRootPath, item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            await galleryRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Gallery item deleted.";
        return RedirectToAction(nameof(Gallery));
    }

    // ═══ AUDIT LOG ════════════════════════════════════════════════════════════
    [Authorize, HttpGet]
    public async Task<IActionResult> AuditLog(int page = 1, CancellationToken ct = default)
    {
        const int size = 50;
        var total = await db.AuditLogs.CountAsync(ct);
        var items = await db.AuditLogs.OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((double)total / size);
        return View(items);
    }

    // ═══ HELPERS ══════════════════════════════════════════════════════════════
    private static void NormalizeCourse(CourseFormViewModel vm)
    {
        vm.Title = vm.Title.Trim();
        vm.TitleMarathi = string.IsNullOrWhiteSpace(vm.TitleMarathi) ? vm.Title : vm.TitleMarathi.Trim();
        vm.Description = vm.Description.Trim();
        vm.DescriptionMarathi = string.IsNullOrWhiteSpace(vm.DescriptionMarathi) ? vm.Description : vm.DescriptionMarathi.Trim();
        vm.Standard = vm.Standard.Trim();
        vm.Slug = string.IsNullOrWhiteSpace(vm.Slug)
            ? vm.Title.Trim().ToLowerInvariant().Replace(" ", "-")
            : vm.Slug.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private static void NormalizeTestimonial(TestimonialFormViewModel vm)
    {
        vm.Name = vm.Name.Trim();
        vm.Role = string.IsNullOrWhiteSpace(vm.Role) ? "Parent / Student" : vm.Role.Trim();
        vm.Quote = vm.Quote.Trim();
        vm.QuoteMarathi = string.IsNullOrWhiteSpace(vm.QuoteMarathi) ? null : vm.QuoteMarathi.Trim();
        vm.Rating = Math.Clamp(vm.Rating, 1, 5);
    }
}
