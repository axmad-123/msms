using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.School;
using MSMS.Domain.Entities;
using MSMS.Domain.Helpers;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class SchoolStructureService(ApplicationDbContext db) : ISchoolStructureService
{
    public async Task<IReadOnlyList<ClassDto>> ListClassesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Classes.AsNoTracking()
            .OrderBy(c => c.AcademicYear).ThenBy(c => c.Name)
            .Select(c => new ClassDto(c.Id, c.Name, c.GradeLevel, c.Section, c.AcademicYear, c.SchoolSection))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassDetailsDto?> GetClassAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schoolClass = await db.Classes.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.GradeLevel,
                c.Section,
                c.AcademicYear,
                c.SchoolSection
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (schoolClass is null)
        {
            return null;
        }

        var students = await db.Students.AsNoTracking()
            .Where(s => s.ClassId == id)
            .OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
            .Select(s => new ClassStudentSummaryDto(s.Id, s.StudentNumber, s.FirstName, s.LastName, s.PhotoUrl))
            .ToListAsync(cancellationToken);

        var subjects = await db.TeacherSubjects.AsNoTracking()
            .Where(ts => ts.ClassId == id)
            .OrderBy(ts => ts.Subject.Name)
            .Select(ts => new ClassSubjectSummaryDto(
                ts.SubjectId,
                ts.Subject.Name,
                ts.TeacherId,
                ts.Teacher.FirstName + " " + ts.Teacher.LastName))
            .ToListAsync(cancellationToken);

        return new ClassDetailsDto(
            schoolClass.Id,
            schoolClass.Name,
            schoolClass.GradeLevel,
            schoolClass.Section,
            schoolClass.AcademicYear,
            schoolClass.SchoolSection,
            students.Count,
            subjects.Select(s => s.SubjectId).Distinct().Count(),
            students,
            subjects);
    }

    public async Task<Guid> CreateClassAsync(CreateClassDto dto, CancellationToken cancellationToken = default)
    {
        var grade = dto.GradeLevel.Trim();
        var entity = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            GradeLevel = grade,
            Section = dto.Section,
            AcademicYear = dto.AcademicYear.Trim(),
            SchoolSection = GradeHelper.SectionFromGrade(grade)
        };

        db.Classes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdateClassAsync(Guid id, CreateClassDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Classes.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Class not found.");

        entity.Name = dto.Name.Trim();
        entity.GradeLevel = dto.GradeLevel.Trim();
        entity.Section = dto.Section;
        entity.AcademicYear = dto.AcademicYear.Trim();
        entity.SchoolSection = GradeHelper.SectionFromGrade(entity.GradeLevel);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteClassAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hasStudents = await db.Students.AnyAsync(s => s.ClassId == id, cancellationToken);
        if (hasStudents)
        {
            throw new InvalidOperationException("Cannot delete a class that still has students assigned.");
        }

        var entity = await db.Classes.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Class not found.");

        db.Classes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubjectDto>> ListSubjectsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Subjects.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SubjectDto(s.Id, s.Name, s.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateSubjectAsync(CreateSubjectDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Subject
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim()
        };

        db.Subjects.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdateSubjectAsync(Guid id, CreateSubjectDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Subjects.SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Subject not found.");

        entity.Name = dto.Name.Trim();
        entity.Code = dto.Code.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSubjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Subjects.SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Subject not found.");

        db.Subjects.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignTeacherAsync(TeacherSubjectAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await db.TeacherSubjects.AnyAsync(
            x => x.TeacherId == dto.TeacherId && x.SubjectId == dto.SubjectId && x.ClassId == dto.ClassId,
            cancellationToken);

        if (exists)
        {
            return;
        }

        db.TeacherSubjects.Add(new TeacherSubject
        {
            TeacherId = dto.TeacherId,
            SubjectId = dto.SubjectId,
            ClassId = dto.ClassId
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnassignTeacherAsync(Guid teacherId, Guid subjectId, Guid classId, CancellationToken cancellationToken = default)
    {
        var link = await db.TeacherSubjects.SingleOrDefaultAsync(
            x => x.TeacherId == teacherId && x.SubjectId == subjectId && x.ClassId == classId,
            cancellationToken);

        if (link is null)
        {
            return;
        }

        db.TeacherSubjects.Remove(link);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherSubjectDto>> ListAssignmentsAsync(Guid? teacherId, CancellationToken cancellationToken = default)
    {
        var query =
            from ts in db.TeacherSubjects.AsNoTracking()
            join t in db.Teachers on ts.TeacherId equals t.Id
            join s in db.Subjects on ts.SubjectId equals s.Id
            join c in db.Classes on ts.ClassId equals c.Id
            where teacherId == null || ts.TeacherId == teacherId.Value
            orderby t.FirstName, t.LastName, s.Name
            select new TeacherSubjectDto(
                ts.TeacherId,
                t.FirstName + " " + t.LastName,
                ts.SubjectId,
                s.Name,
                ts.ClassId,
                c.Name);

        return await query.ToListAsync(cancellationToken);
    }
}
