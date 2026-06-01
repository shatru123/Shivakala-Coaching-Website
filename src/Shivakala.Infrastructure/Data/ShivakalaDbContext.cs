using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data;

public sealed class ShivakalaDbContext(DbContextOptions<ShivakalaDbContext> options) : DbContext(options)
{
    // ── Existing ───────────────────────────────────────────────────────────
    public DbSet<Student>       Students      => Set<Student>();
    public DbSet<Enquiry>       Enquiries     => Set<Enquiry>();
    public DbSet<Course>        Courses       => Set<Course>();
    public DbSet<Notice>        Notices       => Set<Notice>();
    public DbSet<TestResult>    TestResults   => Set<TestResult>();
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
    public DbSet<GalleryItem>   GalleryItems  => Set<GalleryItem>();
    public DbSet<Testimonial>   Testimonials  => Set<Testimonial>();

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
    }
}
