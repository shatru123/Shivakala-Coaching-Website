using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class AdminController(
    IAdminAuthenticationService adminAuthenticationService,
    IAdminPortalService adminPortalService,
    ILogger<AdminController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(AdminLoginViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!adminAuthenticationService.ValidateCredentials(model.Username, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid admin username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username.Trim()),
            new(ClaimTypes.Role, "Admin")
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        logger.LogInformation("Admin logged in as {Username}", model.Username);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Access(string username, string password, string? returnUrl = null)
    {
        if (!adminAuthenticationService.ValidateCredentials(username, password))
        {
            var model = new AdminLoginViewModel
            {
                Username = username,
                ReturnUrl = returnUrl
            };

            ModelState.AddModelError(string.Empty, "Invalid admin username or password.");
            return View("Login", model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username.Trim()),
            new(ClaimTypes.Role, "Admin")
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        logger.LogInformation("Admin logged in as {Username}", username);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await adminPortalService.GetDashboardAsync(cancellationToken));

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Registrations(CancellationToken cancellationToken)
        => View(new AdminListPageViewModel<StudentAdminViewModel>
        {
            Title = "Registrations",
            Items = await adminPortalService.GetRegistrationsAsync(cancellationToken)
        });

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Enquiries(CancellationToken cancellationToken)
        => View(new AdminListPageViewModel<EnquiryAdminViewModel>
        {
            Title = "Enquiries",
            Items = await adminPortalService.GetEnquiriesAsync(cancellationToken)
        });
}
