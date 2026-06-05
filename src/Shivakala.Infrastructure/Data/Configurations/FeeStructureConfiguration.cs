using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(10, 2);
        b.Property(x => x.Standard).IsRequired();
        b.Property(x => x.FeeType).IsRequired();
        b.Property(x => x.AcademicYear).IsRequired();
    }
}
