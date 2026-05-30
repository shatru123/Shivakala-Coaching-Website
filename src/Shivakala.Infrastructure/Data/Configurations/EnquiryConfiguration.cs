using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class EnquiryConfiguration : IEntityTypeConfiguration<Enquiry>
{
    public void Configure(EntityTypeBuilder<Enquiry> builder)
    {
        builder.ToTable("Enquiries");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Mobile).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(600);
        builder.Property(x => x.CreatedDate).IsRequired();
    }
}
