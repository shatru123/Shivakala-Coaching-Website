using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Shivakala.Infrastructure.Data.Seed;
using Shivakala.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x => {
    x.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20 MB (raised for photo/PDF uploads)
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath          = "/admin/login";
        options.AccessDeniedPath   = "/admin/login";
        options.Cookie.Name        = "Shivakala.Auth";
        options.Cookie.HttpOnly    = true;
        options.Cookie.SameSite    = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.ExpireTimeSpan     = TimeSpan.FromHours(8);
        options.SlidingExpiration  = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                var path = ctx.Request.Path.Value ?? "";
                if (path.StartsWith("/teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/teacher/login?returnUrl={returnUrl}");
                    return Task.CompletedTask;
                }
                if (path.StartsWith("/parent", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/parent/login?returnUrl={returnUrl}");
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                var path = ctx.Request.Path.Value ?? "";
                if (path.StartsWith("/teacher", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Redirect("/teacher/login");
                    return Task.CompletedTask;
                }
                if (path.StartsWith("/parent", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Redirect("/parent/login");
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect("/admin/login");
                return Task.CompletedTask;
            }
        };
    });
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("mr")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture  = new RequestCulture("mr");
    options.SupportedCultures      = supportedCultures;
    options.SupportedUICultures    = supportedCultures;
});

var app = builder.Build();

// ── Ensure required directories exist ────────────────────────────────────────
var wwwroot = app.Environment.WebRootPath;
foreach (var dir in new[]
{
    "App_Data",
    Path.Combine(wwwroot, "uploads", "students"),
    Path.Combine(wwwroot, "uploads", "teachers"),
    Path.Combine(wwwroot, "uploads", "homework"),
    Path.Combine(wwwroot, "uploads", "materials"),
    Path.Combine(wwwroot, "uploads", "gallery"),
})
{
    Directory.CreateDirectory(dir);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Database initialisation (runs all pending migrations on startup) ──────────
await DatabaseInitializer.InitializeAsync(app.Services);

await app.RunAsync();
