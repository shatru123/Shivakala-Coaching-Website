using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Shivakala.Infrastructure.Data.Seed;
using Shivakala.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
var dataProtectionPath = Path.Combine(appDataPath, "DataProtection-Keys");
var startupLogPath = Path.Combine(appDataPath, "startup-errors.log");

EnsureDirectory(appDataPath);
EnsureDirectory(dataProtectionPath);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("ShivakalaCoaching");

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
    x.MultipartBodyLengthLimit = 20 * 1024 * 1024);   // 20 MB uploads

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name       = "Shivakala.Auth";
        options.Cookie.HttpOnly   = true;
        options.Cookie.SameSite   = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath         = "/admin/login";   // default fallback
        options.AccessDeniedPath  = "/access-denied"; // fallback (events override first)

        options.Events = new CookieAuthenticationEvents
        {
            // ── OnRedirectToLogin ─────────────────────────────────────────────
            // Fires when an UNAUTHENTICATED user hits a protected route.
            // Send each portal's visitor to the correct login page.
            OnRedirectToLogin = ctx =>
            {
                var path = ctx.Request.Path.Value ?? "";

                if (path.StartsWith("/teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/teacher/login?returnUrl={ret}");
                }
                else if (path.StartsWith("/parent", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/parent/login?returnUrl={ret}");
                }
                else
                {
                    ctx.Response.Redirect(ctx.RedirectUri); // → /admin/login
                }
                return Task.CompletedTask;
            },

            // ── OnRedirectToAccessDenied ──────────────────────────────────────
            // Fires when an AUTHENTICATED user lacks the required ROLE (HTTP 403).
            // IMPORTANT: redirect based on the USER'S ROLE, NOT the URL path.
            //
            //   Teacher visits /admin → role check fails → here we check who they
            //   ARE, not where they tried to go → send them to /teacher (their home).
            //
            OnRedirectToAccessDenied = ctx =>
            {
                var user = ctx.HttpContext.User;

                if (user.IsInRole("Teacher"))
                {
                    // Authenticated teacher tried to access a page outside their role
                    ctx.Response.Redirect("/teacher");
                    return Task.CompletedTask;
                }

                if (user.IsInRole("Parent"))
                {
                    // Authenticated parent tried to access a page outside their role
                    ctx.Response.Redirect("/parent");
                    return Task.CompletedTask;
                }

                // Unknown role or admin trying to access something they can't
                ctx.Response.Redirect("/access-denied");
                return Task.CompletedTask;
            }
        };
    });

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("mr") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("mr");
    options.SupportedCultures     = supportedCultures;
    options.SupportedUICultures   = supportedCultures;
});

var app = builder.Build();

// Ensure upload directories exist on startup
var wwwroot = app.Environment.WebRootPath;
var directories = new List<string>
{
    appDataPath,
    dataProtectionPath,
    Path.Combine(wwwroot, "uploads", "students"),
    Path.Combine(wwwroot, "uploads", "teachers"),
    Path.Combine(wwwroot, "uploads", "homework"),
    Path.Combine(wwwroot, "uploads", "materials"),
    Path.Combine(wwwroot, "uploads", "gallery"),
};

foreach (var dir in directories)
    EnsureDirectory(dir);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization(
    app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    await DatabaseInitializer.InitializeAsync(app.Services);
}
catch (Exception ex) when (!app.Environment.IsDevelopment())
{
    WriteStartupError(startupLogPath, ex);
}

await app.RunAsync();

static void EnsureDirectory(string path)
{
    try
    {
        Directory.CreateDirectory(path);
    }
    catch
    {
        // Best effort only; production hosts can have restrictive filesystem permissions.
    }
}

static void WriteStartupError(string startupLogPath, Exception exception)
{
    var lines = new[]
    {
        $"[{DateTime.UtcNow:O}] Startup initialization failed",
        exception.ToString(),
        string.Empty
    };

    try
    {
        var directory = Path.GetDirectoryName(startupLogPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.AppendAllLines(startupLogPath, lines);
    }
    catch
    {
        // Do not let fallback logging take the site down.
    }

    Console.Error.WriteLine(exception);
}
