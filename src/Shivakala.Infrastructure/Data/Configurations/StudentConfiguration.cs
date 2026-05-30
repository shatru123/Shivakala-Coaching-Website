using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Mobile).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Standard).IsRequired().HasMaxLength(80);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(250);
        builder.Property(x => x.CreatedDate).IsRequired();
    }
}
