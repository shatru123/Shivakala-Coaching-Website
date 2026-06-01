using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;
using Shivakala.Infrastructure.Services;

namespace Shivakala.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=App_Data/shivakala.db";
        var adminSection = configuration.GetSection(AdminCredentialsOptions.SectionName);
        services.Configure<AdminCredentialsOptions>(options => {
            options.Username = adminSection["Username"] ?? "admin";
            options.Password = adminSection["Password"] ?? "P@$$w0rd";
        });

        services.AddDbContext<ShivakalaDbContext>(options => options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnquiryRepository, EnquiryRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<IStudyMaterialRepository, StudyMaterialRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();

        // Services
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IEnquiryService, EnquiryService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IHomePageService, HomePageService>();
        services.AddScoped<IAdminPortalService, AdminPortalService>();
        services.AddSingleton<IAdminAuthenticationService, AdminAuthenticationService>();

        return services;
    }
}
