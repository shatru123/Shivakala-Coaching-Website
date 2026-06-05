using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Configuration;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;
using Shivakala.Infrastructure.Services;

namespace Shivakala.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        var provider = DatabaseProviderResolver.Normalize(configuration[$"{DatabaseOptions.SectionName}:Provider"]);
        var connectionString = GetConnectionString(configuration, provider);

        services.Configure<AdminCredentialsOptions>(options =>
        {
            var section = configuration.GetSection(AdminCredentialsOptions.SectionName);
            options.Username = section["Username"] ?? "admin";
            options.Password = section["Password"] ?? "P@$$w0rd";
        });

        services.AddDbContext<ShivakalaDbContext>(options =>
        {
            if (DatabaseProviderResolver.IsPostgreSql(provider))
            {
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsAssembly("Shivakala.PostgresMigrations"));
                return;
            }

            options.UseSqlite(connectionString,
                sql => sql.MigrationsAssembly("Shivakala.Infrastructure"));
        });

        // ── Existing Repositories ──────────────────────────────────────────
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnquiryRepository, EnquiryRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<IStudyMaterialRepository, StudyMaterialRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();

        // ── New Repositories ───────────────────────────────────────────────
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IHomeworkRepository, HomeworkRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // ── Existing Services ──────────────────────────────────────────────
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IEnquiryService, EnquiryService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IHomePageService, HomePageService>();
        services.AddScoped<IAdminPortalService, AdminPortalService>();
        services.AddScoped<IPortalUserService, PortalUserService>();
        services.AddSingleton<IAdminAuthenticationService, AdminAuthenticationService>();

        // ── New Services ───────────────────────────────────────────────────
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IWhatsAppService, WhatsAppService>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration, string provider)
    {
        if (DatabaseProviderResolver.IsPostgreSql(provider))
        {
            return configuration.GetConnectionString("PostgreSql")
                ?? throw new InvalidOperationException(
                    "Connection string 'PostgreSql' is required when Database:Provider is set to PostgreSql.");
        }

        return configuration.GetConnectionString("Sqlite")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/shivakala.db";
    }
}
