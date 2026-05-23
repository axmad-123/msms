using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class ExamResultService(ApplicationDbContext db, ICurrentUserAccessor currentUser) : IExamResultService
{
    public async Task<IReadOnlyList<ExamResultDto>> GetForStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        await EnsureCanViewStudentResultsAsync(studentId, cancellationToken);

        return await db.ExamResults.AsNoTracking()
            .Where(r => r.StudentId == studentId)
            .Join(db.Students, r => r.StudentId, s => s.Id, (r, s) => new { r, StudentName = s.FirstName + " " + s.LastName })
            .Join(db.Subjects, x => x.r.SubjectId, s => s.Id, (x, s) => new { x.r, x.StudentName, SubjectName = s.Name })
            .OrderBy(x => x.r.ExamType)
            .Select(x => new ExamResultDto(
                x.r.Id,
                x.r.StudentId,
                x.StudentName,
                x.r.SubjectId,
                x.SubjectName,
                (int)x.r.ExamType,
                x.r.Marks,
                x.r.MaxMarks,
                x.r.Grade,
                x.r.AcademicYear,
                x.r.EnteredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResultDto>> GetForClassSubjectAsync(Guid classId, Guid subjectId, int? examType, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsInRole(RoleNames.Admin) && !currentUser.IsInRole(RoleNames.Teacher))
        {
            throw new UnauthorizedAccessException();
        }

        var query =
            from r in db.ExamResults.AsNoTracking()
            join st in db.Students on r.StudentId equals st.Id
            join su in db.Subjects on r.SubjectId equals su.Id
            where st.ClassId == classId && r.SubjectId == subjectId
            select new { r, StudentName = st.FirstName + " " + st.LastName, SubjectName = su.Name };

        if (examType is not null)
        {
            if (!Enum.IsDefined(typeof(ExamType), examType.Value))
            {
                throw new InvalidOperationException("Invalid exam type filter.");
            }

            var et = (ExamType)examType.Value;
            query = query.Where(x => x.r.ExamType == et);
        }

        if (currentUser.IsInRole(RoleNames.Teacher))
        {
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var teacherId = await db.Teachers.Where(t => t.UserId == userId).Select(t => t.Id).SingleAsync(cancellationToken);
            var allowed = await db.TeacherSubjects.AnyAsync(ts => ts.TeacherId == teacherId && ts.ClassId == classId && ts.SubjectId == subjectId, cancellationToken);
            if (!allowed)
            {
                throw new UnauthorizedAccessException("You are not assigned to this class and subject.");
            }
        }

        return await query
            .OrderBy(x => x.r.ExamType).ThenBy(x => x.StudentName)
            .Select(x => new ExamResultDto(
                x.r.Id,
                x.r.StudentId,
                x.StudentName,
                x.r.SubjectId,
                x.SubjectName,
                (int)x.r.ExamType,
                x.r.Marks,
                x.r.MaxMarks,
                x.r.Grade,
                x.r.AcademicYear,
                x.r.EnteredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertResultsAsync(UpsertExamResultsDto dto, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsInRole(RoleNames.Teacher))
        {
            throw new UnauthorizedAccessException("Only teachers can enter exam results.");
        }

        if (!Enum.IsDefined(typeof(ExamType), dto.ExamType))
        {
            throw new InvalidOperationException("Invalid exam type.");
        }

        var userId = currentUser.UserId ?? throw new InvalidOperationException();
        var teacher = await db.Teachers.SingleAsync(t => t.UserId == userId, cancellationToken);
        var allowed = await db.TeacherSubjects.AnyAsync(
            ts => ts.TeacherId == teacher.Id && ts.ClassId == dto.ClassId && ts.SubjectId == dto.SubjectId,
            cancellationToken);

        if (!allowed)
        {
            throw new UnauthorizedAccessException("You are not assigned to this class and subject.");
        }

        var examType = (ExamType)dto.ExamType;
        var studentIds = dto.Lines.Select(l => l.StudentId).Distinct().ToArray();
        var valid = await db.Students.CountAsync(
            s => studentIds.Contains(s.Id) && s.ClassId == dto.ClassId && s.Status == StudentStatus.Active,
            cancellationToken);

        if (valid != studentIds.Length)
        {
            throw new InvalidOperationException("One or more students are not active members of this class.");
        }

        var existing = await db.ExamResults
            .Where(r => r.SubjectId == dto.SubjectId && r.ExamType == examType && r.AcademicYear == dto.AcademicYear && studentIds.Contains(r.StudentId))
            .ToListAsync(cancellationToken);

        foreach (var line in dto.Lines)
        {
            var row = existing.SingleOrDefault(r => r.StudentId == line.StudentId);
            if (row is null)
            {
                db.ExamResults.Add(new ExamResult
                {
                    Id = Guid.NewGuid(),
                    StudentId = line.StudentId,
                    SubjectId = dto.SubjectId,
                    TeacherId = teacher.Id,
                    ExamType = examType,
                    Marks = line.Marks,
                    MaxMarks = line.MaxMarks,
                    Grade = line.Grade.Trim(),
                    AcademicYear = dto.AcademicYear.Trim(),
                    EnteredAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                row.Marks = line.Marks;
                row.MaxMarks = line.MaxMarks;
                row.Grade = line.Grade.Trim();
                row.TeacherId = teacher.Id;
                row.EnteredAtUtc = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCanViewStudentResultsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (currentUser.IsInRole(RoleNames.Student))
        {
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var ok = await db.Students.AnyAsync(s => s.Id == studentId && s.UserId == userId, cancellationToken);
            if (!ok)
            {
                throw new UnauthorizedAccessException();
            }

            return;
        }

        if (currentUser.IsInRole(RoleNames.Parent))
        {
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var parentId = await db.Parents.Where(p => p.UserId == userId).Select(p => p.Id).SingleAsync(cancellationToken);
            var ok = await db.ParentChildren.AnyAsync(l => l.ParentId == parentId && l.StudentId == studentId, cancellationToken);
            if (!ok)
            {
                throw new UnauthorizedAccessException();
            }

            return;
        }

        if (currentUser.IsInRole(RoleNames.Teacher))
        {
            // Teachers can view results for students in classes they teach (any subject)
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var teacherId = await db.Teachers.Where(t => t.UserId == userId).Select(t => t.Id).SingleAsync(cancellationToken);
            var classId = await db.Students.Where(s => s.Id == studentId).Select(s => s.ClassId).SingleAsync(cancellationToken);
            if (classId is null)
            {
                throw new InvalidOperationException("Student is not assigned to a class.");
            }

            var ok = await db.TeacherSubjects.AnyAsync(ts => ts.TeacherId == teacherId && ts.ClassId == classId, cancellationToken);
            if (!ok)
            {
                throw new UnauthorizedAccessException();
            }

            return;
        }

        throw new UnauthorizedAccessException();
    }
}
