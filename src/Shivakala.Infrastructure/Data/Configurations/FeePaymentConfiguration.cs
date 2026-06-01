using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class FeePaymentConfiguration : IEntityTypeConfiguration<FeePayment>
{
    public void Configure(EntityTypeBuilder<FeePayment> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(10,2)");
        b.Property(x => x.Discount).HasColumnType("decimal(10,2)");
        b.Property(x => x.Fine).HasColumnType("decimal(10,2)");
        b.Property(x => x.PaidAmount).HasColumnType("decimal(10,2)");
        b.HasOne(x => x.Student).WithMany(s => s.FeePayments).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ReceiptNumber).IsUnique();
    }
}
