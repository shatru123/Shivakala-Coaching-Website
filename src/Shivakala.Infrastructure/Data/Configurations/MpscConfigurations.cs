using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class MpscStudentConfiguration : IEntityTypeConfiguration<MpscStudent>
{
    public void Configure(EntityTypeBuilder<MpscStudent> b)
    {
        b.ToTable("MpscStudents");
        b.HasKey(x => x.Id);

        b.HasIndex(x => x.RegistrationNumber).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.Mobile).IsUnique();

        b.Property(x => x.RegistrationNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.FullNameMarathi).HasMaxLength(150);
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
        b.Property(x => x.Mobile).HasMaxLength(15).IsRequired();
        b.Property(x => x.AlternateMobile).HasMaxLength(15);
        b.Property(x => x.DateOfBirth).HasMaxLength(20).IsRequired();
        b.Property(x => x.Gender).HasMaxLength(20).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.District).HasMaxLength(100).IsRequired();
        b.Property(x => x.State).HasMaxLength(100).IsRequired().HasDefaultValue("Maharashtra");
        b.Property(x => x.Pincode).HasMaxLength(10);
        b.Property(x => x.HighestQualification).HasMaxLength(100).IsRequired();
        b.Property(x => x.DegreeOrCourse).HasMaxLength(150);
        b.Property(x => x.University).HasMaxLength(200);
        b.Property(x => x.PreferredExam).HasMaxLength(100).IsRequired().HasDefaultValue("Rajyaseva");
        b.Property(x => x.OtherExamInterest).HasMaxLength(250);
        b.Property(x => x.PreparationLevel).HasMaxLength(50).IsRequired().HasDefaultValue("Beginner");
        b.Property(x => x.Category).HasMaxLength(50).IsRequired().HasDefaultValue("Open");

        b.HasMany(x => x.Registrations)
            .WithOne(r => r.MpscStudent)
            .HasForeignKey(r => r.MpscStudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Results)
            .WithOne(r => r.MpscStudent)
            .HasForeignKey(r => r.MpscStudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.OnlineAttempts)
            .WithOne(a => a.MpscStudent)
            .HasForeignKey(a => a.MpscStudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MpscExamRegistrationConfiguration : IEntityTypeConfiguration<MpscExamRegistration>
{
    public void Configure(EntityTypeBuilder<MpscExamRegistration> b)
    {
        b.ToTable("MpscExamRegistrations");
        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.ExamId, x.MpscStudentId }).IsUnique();

        b.Property(x => x.Status).HasMaxLength(50).IsRequired().HasDefaultValue("Registered");
        b.Property(x => x.Mode).HasMaxLength(50).IsRequired().HasDefaultValue("Online");
        b.Property(x => x.SeatNumber).HasMaxLength(50);
        b.Property(x => x.AttendanceStatus).HasMaxLength(50).IsRequired().HasDefaultValue("Pending");
        b.Property(x => x.AttendanceRemarks).HasMaxLength(500);

        b.HasOne(x => x.Exam)
            .WithMany(e => e.MpscRegistrations)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MpscStudent)
            .WithMany(s => s.Registrations)
            .HasForeignKey(x => x.MpscStudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MpscExamResultConfiguration : IEntityTypeConfiguration<MpscExamResult>
{
    public void Configure(EntityTypeBuilder<MpscExamResult> b)
    {
        b.ToTable("MpscExamResults");
        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.ExamId, x.MpscStudentId });

        b.Property(x => x.Grade).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.Property(x => x.Mode).HasMaxLength(50).IsRequired().HasDefaultValue("Offline");

        b.HasOne(x => x.Exam)
            .WithMany(e => e.MpscResults)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MpscStudent)
            .WithMany(s => s.Results)
            .HasForeignKey(x => x.MpscStudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.OnlineAttempt)
            .WithMany()
            .HasForeignKey(x => x.OnlineAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

