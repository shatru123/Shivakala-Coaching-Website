using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

public sealed class AdminController(
    IAdminAuthenticationService authService,
    IAdminPortalService portalService,
    INoticeRepository noticeRepo,
    ITestResultRepository resultRepo,
    IStudyMaterialRepository materialRepo,
    IGalleryRepository galleryRepo,
    ITestimonialRepository testimonialRepo,
    ILogger<AdminController> logger) : Controller
{
    // ===== AUTH =====
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
        var claims = new List<Claim> { new(ClaimTypes.Name, model.Username.Trim()), new(ClaimTypes.Role, "Admin") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        logger.LogInformation("Admin logged in: {User}", model.Username);
        return !string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ===== DASHBOARD =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await portalService.GetDashboardAsync(ct));

    // ===== REGISTRATIONS =====
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

    // ===== ENQUIRIES =====
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

    // ===== NOTICES =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Notices(CancellationToken ct)
        => View(await noticeRepo.GetAllAdminAsync(ct));

    [Authorize, HttpGet]
    public IActionResult CreateNotice() => View("NoticeForm", new NoticeFormViewModel { PublishedDate = DateTime.Today });

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        await noticeRepo.AddAsync(new Core.Entities.Notice {
            Title=vm.Title, TitleMarathi=vm.TitleMarathi, Body=vm.Body,
            BodyMarathi=vm.BodyMarathi, Category=vm.Category, IsPinned=vm.IsPinned,
            IsActive=vm.IsActive, PublishedDate=vm.PublishedDate, CreatedDate=DateTime.UtcNow
        }, ct);
        TempData["SuccessMessage"] = "Notice created successfully.";
        return RedirectToAction(nameof(Notices));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditNotice(int id, CancellationToken ct)
    {
        var n = await noticeRepo.GetByIdAsync(id, ct);
        if (n == null) return NotFound();
        return View("NoticeForm", new NoticeFormViewModel {
            Id=n.Id, Title=n.Title, TitleMarathi=n.TitleMarathi, Body=n.Body,
            BodyMarathi=n.BodyMarathi, Category=n.Category, IsPinned=n.IsPinned,
            IsActive=n.IsActive, PublishedDate=n.PublishedDate
        });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNotice(NoticeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("NoticeForm", vm);
        var n = await noticeRepo.GetByIdAsync(vm.Id, ct);
        if (n == null) return NotFound();
        n.Title=vm.Title; n.TitleMarathi=vm.TitleMarathi; n.Body=vm.Body;
        n.BodyMarathi=vm.BodyMarathi; n.Category=vm.Category; n.IsPinned=vm.IsPinned;
        n.IsActive=vm.IsActive; n.PublishedDate=vm.PublishedDate;
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

    // ===== RESULTS =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Results(CancellationToken ct)
        => View(await resultRepo.GetAllAdminAsync(ct));

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
            StudentName=vm.StudentName, Standard=vm.Standard, Subject=vm.Subject,
            Score=vm.Score, TotalMarks=vm.TotalMarks, Rank=vm.Rank, Grade=grade,
            Remarks=vm.Remarks, TestDate=vm.TestDate, TestTitle=vm.TestTitle, CreatedDate=DateTime.UtcNow
        }, ct);
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

    // ===== STUDY MATERIALS =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Materials(CancellationToken ct)
        => View(await materialRepo.GetAllAdminAsync(ct));

    [Authorize, HttpGet]
    public IActionResult CreateMaterial() => View("MaterialForm", new StudyMaterialFormViewModel());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMaterial(StudyMaterialFormViewModel vm, IWebHostEnvironment env, CancellationToken ct)
    {
        if (vm.File == null || vm.File.Length == 0) ModelState.AddModelError("File","Please upload a file.");
        if (!ModelState.IsValid) return View("MaterialForm", vm);

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "materials");
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.File!.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);
        await using var stream = System.IO.File.Create(filePath);
        await vm.File.CopyToAsync(stream, ct);

        await materialRepo.AddAsync(new Core.Entities.StudyMaterial {
            Title=vm.Title, TitleMarathi=vm.TitleMarathi, FileUrl=$"/uploads/materials/{fileName}",
            Standard=vm.Standard, Subject=vm.Subject, MaterialType=vm.MaterialType,
            FileSizeBytes=vm.File.Length, IsActive=vm.IsActive, UploadedDate=DateTime.UtcNow
        }, ct);
        TempData["SuccessMessage"] = "Study material uploaded.";
        return RedirectToAction(nameof(Materials));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaterial(int id, IWebHostEnvironment env, CancellationToken ct)
    {
        var m = await materialRepo.GetByIdAsync(id, ct);
        if (m != null)
        {
            var physPath = Path.Combine(env.WebRootPath, m.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physPath)) System.IO.File.Delete(physPath);
            await materialRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Material deleted.";
        return RedirectToAction(nameof(Materials));
    }

    // ===== TESTIMONIALS =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Testimonials(CancellationToken ct)
        => View(await testimonialRepo.GetAllAdminAsync(ct));

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTestimonial(int id, bool featured, CancellationToken ct)
    {
        var t = await testimonialRepo.GetByIdAsync(id, ct);
        if (t != null) { t.IsApproved = true; t.IsFeatured = featured; await testimonialRepo.UpdateAsync(t, ct); }
        TempData["SuccessMessage"] = "Testimonial approved.";
        return RedirectToAction(nameof(Testimonials));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestimonial(int id, CancellationToken ct)
    {
        await testimonialRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Testimonial deleted.";
        return RedirectToAction(nameof(Testimonials));
    }

    // ===== GALLERY =====
    [Authorize, HttpGet]
    public async Task<IActionResult> Gallery(CancellationToken ct)
        => View("AdminGallery", await galleryRepo.GetAllAdminAsync(ct));

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadGalleryItem(string title, string category, string? caption, IFormFile image, IWebHostEnvironment env, CancellationToken ct)
    {
        if (image != null && image.Length > 0)
        {
            var dir = Path.Combine(env.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            await using var s = System.IO.File.Create(Path.Combine(dir, fileName));
            await image.CopyToAsync(s, ct);
            await galleryRepo.AddAsync(new Core.Entities.GalleryItem {
                Title=title, ImageUrl=$"/uploads/gallery/{fileName}", Caption=caption,
                Category=string.IsNullOrWhiteSpace(category)?"General":category,
                DisplayOrder=99, IsActive=true, CreatedDate=DateTime.UtcNow
            }, ct);
        }
        TempData["SuccessMessage"] = "Image uploaded.";
        return RedirectToAction(nameof(Gallery));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGalleryItem(int id, IWebHostEnvironment env, CancellationToken ct)
    {
        var g = await galleryRepo.GetAllAdminAsync(ct);
        var item = g.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            var ph = Path.Combine(env.WebRootPath, item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(ph)) System.IO.File.Delete(ph);
            await galleryRepo.DeleteAsync(id, ct);
        }
        TempData["SuccessMessage"] = "Gallery item deleted.";
        return RedirectToAction(nameof(Gallery));
    }
}
