using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasMany(x => x.BatchSubjects).WithOne(s => s.Batch).HasForeignKey(s => s.BatchId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.StudentBatches).WithOne(s => s.Batch).HasForeignKey(s => s.BatchId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.TimetableSlots).WithOne(s => s.Batch).HasForeignKey(s => s.BatchId).OnDelete(DeleteBehavior.Cascade);
    }
}
