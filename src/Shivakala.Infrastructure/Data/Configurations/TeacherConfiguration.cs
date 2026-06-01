using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        b.Property(x => x.MonthlySalary).HasColumnType("decimal(10,2)");
    }
}
