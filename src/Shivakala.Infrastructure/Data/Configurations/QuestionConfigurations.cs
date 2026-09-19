using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.QuestionText).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(100).IsRequired();
        b.Property(x => x.Standard).HasMaxLength(50).IsRequired();
        b.Property(x => x.Difficulty).HasMaxLength(20).HasDefaultValue("Medium");
        b.HasMany(x => x.Options).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.OptionText).IsRequired();
        b.Property(x => x.OptionKey).HasMaxLength(10).IsRequired();
    }
}

public sealed class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
{
    public void Configure(EntityTypeBuilder<ExamQuestion> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamId, x.QuestionId }).IsUnique();
        b.HasOne(x => x.Exam).WithMany(e => e.ExamQuestions).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Question).WithMany(q => q.ExamQuestions).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ExamRegistrationConfiguration : IEntityTypeConfiguration<ExamRegistration>
{
    public void Configure(EntityTypeBuilder<ExamRegistration> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamId, x.StudentId }).IsUnique();
        b.HasOne(x => x.Exam).WithMany(e => e.Registrations).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OnlineExamAttemptConfiguration : IEntityTypeConfiguration<OnlineExamAttempt>
{
    public void Configure(EntityTypeBuilder<OnlineExamAttempt> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamId, x.StudentId });
        b.HasIndex(x => new { x.ExamId, x.MpscStudentId });
        b.HasOne(x => x.Exam).WithMany(e => e.Attempts).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MpscStudent).WithMany(m => m.OnlineAttempts).HasForeignKey(x => x.MpscStudentId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Answers).WithOne(a => a.Attempt).HasForeignKey(a => a.AttemptId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OnlineExamAttemptAnswerConfiguration : IEntityTypeConfiguration<OnlineExamAttemptAnswer>
{
    public void Configure(EntityTypeBuilder<OnlineExamAttemptAnswer> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AttemptId, x.QuestionId }).IsUnique();
        b.HasOne(x => x.Question).WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SelectedOption).WithMany().HasForeignKey(x => x.SelectedOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
