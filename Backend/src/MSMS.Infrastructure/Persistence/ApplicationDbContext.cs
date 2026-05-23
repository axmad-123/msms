using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MSMS.Domain.Entities;
using MSMS.Infrastructure.Identity;

namespace MSMS.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<ParentChild> ParentChildren => Set<ParentChild>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<MonthlyFee> MonthlyFees => Set<MonthlyFee>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StudentPromotion> StudentPromotions => Set<StudentPromotion>();
    public DbSet<GraduatedStudent> GraduatedStudents => Set<GraduatedStudent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>(e =>
        {
            e.HasIndex(x => x.StudentNumber).IsUnique();
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.Class)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Parent>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<Administrator>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<Teacher>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
        });

        builder.Entity<ParentChild>(e =>
        {
            e.HasKey(x => new { x.ParentId, x.StudentId });
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId);
            e.HasOne(x => x.Student).WithMany(x => x.ParentLinks).HasForeignKey(x => x.StudentId);
        });

        builder.Entity<TeacherSubject>(e =>
        {
            e.HasKey(x => new { x.TeacherId, x.SubjectId, x.ClassId });
            e.HasOne(x => x.Teacher).WithMany(x => x.Assignments).HasForeignKey(x => x.TeacherId);
            e.HasOne(x => x.Subject).WithMany(x => x.TeacherSubjects).HasForeignKey(x => x.SubjectId);
            e.HasOne(x => x.Class).WithMany(x => x.TeacherSubjects).HasForeignKey(x => x.ClassId);
        });

        builder.Entity<AttendanceSession>(e =>
        {
            e.HasIndex(x => new { x.ClassId, x.SessionDate, x.SessionNumber }).IsUnique();
            e.HasOne(x => x.Class).WithMany(x => x.AttendanceSessions).HasForeignKey(x => x.ClassId);
        });

        builder.Entity<AttendanceRecord>(e =>
        {
            e.HasIndex(x => new { x.AttendanceSessionId, x.StudentId }).IsUnique();
            e.HasOne(x => x.Session).WithMany(x => x.Records).HasForeignKey(x => x.AttendanceSessionId);
            e.HasOne(x => x.Student).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.RecordedByTeacher).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.RecordedByTeacherId);
        });

        builder.Entity<ExamResult>(e =>
        {
            e.HasIndex(x => new { x.StudentId, x.SubjectId, x.ExamType, x.AcademicYear }).IsUnique();
            e.HasOne(x => x.Student).WithMany(x => x.ExamResults).HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.Subject).WithMany(x => x.ExamResults).HasForeignKey(x => x.SubjectId);
            e.HasOne(x => x.Teacher).WithMany(x => x.ExamResults).HasForeignKey(x => x.TeacherId);
        });

        builder.Entity<MonthlyFee>(e =>
        {
            e.HasIndex(x => new { x.StudentId, x.Year, x.Month, x.AcademicYear }).IsUnique();
            e.HasOne(x => x.Student).WithMany(x => x.MonthlyFees).HasForeignKey(x => x.StudentId);
        });

        builder.Entity<Payment>(e =>
        {
            e.HasOne(x => x.Student).WithMany(x => x.Payments).HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.RecordedByAdmin).WithMany().HasForeignKey(x => x.RecordedByAdminId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentPromotion>(e =>
        {
            e.HasOne(x => x.Student).WithMany(x => x.Promotions).HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.FromClass).WithMany().HasForeignKey(x => x.FromClassId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ToClass).WithMany().HasForeignKey(x => x.ToClassId);
        });

        builder.Entity<GraduatedStudent>(e =>
        {
            e.HasOne(x => x.FinalClass).WithMany().HasForeignKey(x => x.FinalClassId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(x => x.Token).IsUnique();
        });
    }
}
