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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/shivakala.db";

        services.AddDbContext<ShivakalaDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnquiryRepository, EnquiryRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IEnquiryService, EnquiryService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IHomePageService, HomePageService>();

        return services;
    }
}
