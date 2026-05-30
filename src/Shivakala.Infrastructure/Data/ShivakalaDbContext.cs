using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data;

public sealed class ShivakalaDbContext(DbContextOptions<ShivakalaDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enquiry> Enquiries => Set<Enquiry>();

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShivakalaDbContext).Assembly);
    }
}
