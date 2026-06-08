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
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));

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

            if (DatabaseProviderResolver.IsSqlServer(provider))
            {
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly("Shivakala.SqlServerMigrations"));
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
        services.AddScoped<IAboutPageService, AboutPageService>();
        services.AddScoped<IAdminPortalService, AdminPortalService>();
        services.AddScoped<IPortalUserService, PortalUserService>();
        services.AddScoped<IAdminAuthenticationService, AdminAuthenticationService>();

        // ── New Services ───────────────────────────────────────────────────
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IWhatsAppService, WhatsAppService>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration, string provider)
    {
        if (DatabaseProviderResolver.IsPostgreSql(provider))
        {
            var databaseUrl = configuration["DATABASE_URL"];
            if (!string.IsNullOrWhiteSpace(databaseUrl))
                return BuildPostgreSqlConnectionStringFromUrl(databaseUrl);

            return configuration.GetConnectionString("PostgreSql")
                ?? throw new InvalidOperationException(
                    "Connection string 'PostgreSql' or environment variable 'DATABASE_URL' is required when Database:Provider is set to PostgreSql.");
        }

        if (DatabaseProviderResolver.IsSqlServer(provider))
        {
            return configuration.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException(
                    "Connection string 'SqlServer' is required when Database:Provider is set to SqlServer.");
        }

        return configuration.GetConnectionString("Sqlite")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/shivakala.db";
    }

    private static string BuildPostgreSqlConnectionStringFromUrl(string databaseUrl)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("Environment variable 'DATABASE_URL' is not a valid absolute URI.");

        if (!string.Equals(uri.Scheme, "postgres", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, "postgresql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Environment variable 'DATABASE_URL' must start with 'postgres://' or 'postgresql://'.");
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException("Environment variable 'DATABASE_URL' must include username and database name.");

        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = username,
            Password = password,
            Database = database,
            SslMode = Npgsql.SslMode.Prefer
        };

        return builder.ConnectionString;
    }
}
