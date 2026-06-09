using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data;

public sealed class ShivakalaDbContext(DbContextOptions<ShivakalaDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter = new(
        value => UtcDateTime.EnsureUtc(value),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcDateTimeConverter = new(
        value => value.HasValue ? UtcDateTime.EnsureUtc(value.Value) : value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);

    // ── Existing ───────────────────────────────────────────────────────────
    public DbSet<Student>       Students      => Set<Student>();
    public DbSet<Enquiry>       Enquiries     => Set<Enquiry>();
    public DbSet<Course>        Courses       => Set<Course>();
    public DbSet<Notice>        Notices       => Set<Notice>();
    public DbSet<TestResult>    TestResults   => Set<TestResult>();
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
    public DbSet<GalleryItem>   GalleryItems  => Set<GalleryItem>();
    public DbSet<Testimonial>   Testimonials  => Set<Testimonial>();
    public DbSet<HomePageSectionSettings> HomePageSectionSettings => Set<HomePageSectionSettings>();
    public DbSet<AboutPageSectionSettings> AboutPageSectionSettings => Set<AboutPageSectionSettings>();

    // ── New ────────────────────────────────────────────────────────────────
    public DbSet<AppUser>           AppUsers           => Set<AppUser>();
    public DbSet<Teacher>           Teachers           => Set<Teacher>();
    public DbSet<Batch>             Batches            => Set<Batch>();
    public DbSet<BatchSubject>      BatchSubjects      => Set<BatchSubject>();
    public DbSet<StudentBatch>      StudentBatches     => Set<StudentBatch>();
    public DbSet<Attendance>        Attendances        => Set<Attendance>();
    public DbSet<TeacherAttendance> TeacherAttendances => Set<TeacherAttendance>();
    public DbSet<FeeStructure>      FeeStructures      => Set<FeeStructure>();
    public DbSet<FeePayment>        FeePayments        => Set<FeePayment>();
    public DbSet<Exam>              Exams              => Set<Exam>();
    public DbSet<ExamResult>        ExamResults        => Set<ExamResult>();
    public DbSet<Homework>          Homeworks          => Set<Homework>();
    public DbSet<HomeworkSubmission> HomeworkSubmissions => Set<HomeworkSubmission>();
    public DbSet<TimetableSlot>     TimetableSlots     => Set<TimetableSlot>();
    public DbSet<Notification>      Notifications      => Set<Notification>();
    public DbSet<AuditLog>          AuditLogs          => Set<AuditLog>();
    public DbSet<SyllabusItem>      SyllabusItems      => Set<SyllabusItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShivakalaDbContext).Assembly);
        ApplyUtcDateTimeConverters(modelBuilder);
    }

    public override int SaveChanges()
    {
        NormalizeDateTimes();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeDateTimes();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimes();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static void ApplyUtcDateTimeConverters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(UtcDateTimeConverter);
                    continue;
                }

                if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(NullableUtcDateTimeConverter);
            }
        }
    }

    private void NormalizeDateTimes()
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dateTimeValue)
                {
                    property.CurrentValue = UtcDateTime.EnsureUtc(dateTimeValue);
                    continue;
                }

                if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue is DateTime nullableDateTimeValue)
                    property.CurrentValue = UtcDateTime.EnsureUtc(nullableDateTimeValue);
            }
        }
    }
}
