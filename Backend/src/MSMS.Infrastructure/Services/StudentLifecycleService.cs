using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Lifecycle;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Domain.Helpers;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class StudentLifecycleService(ApplicationDbContext db, ICurrentUserAccessor currentUser) : IStudentLifecycleService
{
    public async Task PromoteAsync(PromoteStudentDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();

        var student = await db.Students.SingleOrDefaultAsync(s => s.Id == dto.StudentId, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        if (student.Status != StudentStatus.Active)
        {
            throw new InvalidOperationException("Only active students can be promoted.");
        }

        if (!await db.Classes.AnyAsync(c => c.Id == dto.ToClassId, cancellationToken))
        {
            throw new InvalidOperationException("Target class not found.");
        }

        db.StudentPromotions.Add(new StudentPromotion
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            FromClassId = student.ClassId,
            ToClassId = dto.ToClassId,
            PromotionDate = dto.PromotionDate,
            AcademicYear = dto.AcademicYear.Trim()
        });

        student.ClassId = dto.ToClassId;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task GraduateAsync(GraduateStudentDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();

        var student = await db.Students.SingleOrDefaultAsync(s => s.Id == dto.StudentId, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        if (student.Status != StudentStatus.Active)
        {
            throw new InvalidOperationException("Only active students can graduate.");
        }

        if (student.ClassId is null)
        {
            throw new InvalidOperationException("Student must have a class assigned before graduation.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var fullName = $"{student.FirstName} {student.LastName}".Trim();
        db.GraduatedStudents.Add(new GraduatedStudent
        {
            Id = Guid.NewGuid(),
            OriginalStudentId = student.Id,
            UserId = student.UserId,
            StudentNumber = student.StudentNumber,
            FullName = fullName,
            GraduationDate = dto.GraduationDate,
            FinalClassId = student.ClassId.Value,
            AcademicYear = dto.AcademicYear.Trim(),
            FinalStatus = StudentStatus.Graduated,
            ArchivedAtUtc = DateTime.UtcNow
        });

        student.Status = StudentStatus.Graduated;
        student.ClassId = null;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GraduatedStudentDto>> ListGraduatedAsync(CancellationToken cancellationToken = default)
    {
        RequireAdmin();

        return await db.GraduatedStudents.AsNoTracking()
            .OrderByDescending(x => x.GraduationDate)
            .Select(x => new GraduatedStudentDto(
                x.Id,
                x.OriginalStudentId,
                x.StudentNumber,
                x.FullName,
                x.GraduationDate,
                x.FinalClassId,
                x.AcademicYear,
                x.ArchivedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<AutoPromoteResultDto> AutoPromoteAllAsync(AutoPromoteRequestDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var messages = new List<string>();
        var promoted = 0;
        var graduated = 0;
        var newYear = dto.NewAcademicYear.Trim();

        var students = await db.Students
            .Include(s => s.Class)
            .Where(s => s.Status == StudentStatus.Active && s.ClassId != null)
            .ToListAsync(cancellationToken);

        foreach (var student in students)
        {
            var currentClass = student.Class!;
            if (!int.TryParse(currentClass.GradeLevel, out var grade))
            {
                messages.Add($"Skipped {student.StudentNumber}: invalid grade.");
                continue;
            }

            if (grade >= 12)
            {
                await GraduateAsync(new GraduateStudentDto(student.Id, dto.PromotionDate, newYear), cancellationToken);
                graduated++;
                messages.Add($"Graduated {student.FirstName} {student.LastName} (grade 12).");
                continue;
            }

            var nextGrade = (grade + 1).ToString();
            var nextClass = await db.Classes.AsNoTracking()
                .Where(c => c.AcademicYear == newYear && c.GradeLevel == nextGrade && c.Section == currentClass.Section)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextClass == Guid.Empty)
            {
                nextClass = await db.Classes.AsNoTracking()
                    .Where(c => c.AcademicYear == newYear && c.GradeLevel == nextGrade)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (nextClass == Guid.Empty)
            {
                messages.Add($"No class for grade {nextGrade} ({newYear}) — skipped {student.StudentNumber}.");
                continue;
            }

            db.StudentPromotions.Add(new StudentPromotion
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                FromClassId = student.ClassId,
                ToClassId = nextClass,
                PromotionDate = dto.PromotionDate,
                AcademicYear = newYear
            });
            student.ClassId = nextClass;
            promoted++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new AutoPromoteResultDto(promoted, graduated, messages);
    }

    private void RequireAdmin()
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new UnauthorizedAccessException("Administrator access is required.");
        }
    }
}
