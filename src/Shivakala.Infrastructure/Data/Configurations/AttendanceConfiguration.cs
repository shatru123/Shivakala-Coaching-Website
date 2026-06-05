using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.StudentId, x.BatchId, x.Date, x.Subject }).IsUnique();
        b.HasOne(x => x.Student).WithMany(s => s.Attendances).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
