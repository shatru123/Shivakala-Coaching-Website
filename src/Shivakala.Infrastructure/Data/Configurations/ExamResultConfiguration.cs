using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamId, x.StudentId }).IsUnique();
        b.HasOne(x => x.Exam).WithMany(e => e.Results).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Student).WithMany(s => s.ExamResults).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}
