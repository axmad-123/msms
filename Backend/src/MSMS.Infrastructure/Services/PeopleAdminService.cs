using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Common;
using MSMS.Application.DTOs.People;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Domain.Helpers;
using MSMS.Infrastructure.Identity;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class PeopleAdminService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) : IPeopleAdminService
{
    public async Task<PagedResult<StudentListItemDto>> ListStudentsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? int.MaxValue : Math.Clamp(pageSize, 1, 500);

        var query = db.Students.AsNoTracking()
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentListItemDto(
                s.Id,
                s.StudentNumber,
                s.FirstName,
                s.LastName,
                s.ClassId,
                s.Status,
                s.PhotoUrl,
                s.Gender,
                s.Class != null ? s.Class.Name : null,
                s.Class != null ? s.Class.GradeLevel : null))
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentListItemDto>(items, total, page, pageSize);
    }

    public async Task<StudentDetailsDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query =
            from s in db.Students.AsNoTracking()
            join u in db.Users.AsNoTracking() on s.UserId equals u.Id
            where s.Id == id
            select new StudentDetailsDto(
                s.Id,
                s.UserId,
                s.StudentNumber,
                s.FirstName,
                s.LastName,
                s.DateOfBirth,
                s.PlaceOfBirth,
                s.Gender,
                s.PhotoUrl,
                s.ClassId,
                s.Status,
                s.Class != null ? s.Class.Name : null,
                s.Class != null ? s.Class.GradeLevel : null,
                s.Class != null ? s.Class.SchoolSection : null,
                u.Email);

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<decimal> GetSuggestedMonthlyFeeAsync(Guid? classId, CancellationToken cancellationToken = default)
    {
        if (classId is null)
        {
            return 40m;
        }

        var grade = await db.Classes.AsNoTracking()
            .Where(c => c.Id == classId)
            .Select(c => c.GradeLevel)
            .SingleOrDefaultAsync(cancellationToken);

        return grade is null ? 40m : GradeHelper.DefaultMonthlyFee(grade);
    }

    public async Task<CreateStudentResultDto> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var create = await userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, RoleNames.Student);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StudentNumber = dto.StudentNumber.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DateOfBirth = dto.DateOfBirth,
            PlaceOfBirth = dto.PlaceOfBirth?.Trim(),
            Gender = dto.Gender,
            PhotoUrl = dto.PhotoUrl,
            ClassId = dto.ClassId,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var feeAmount = dto.MonthlyFeeAmount is > 0
            ? dto.MonthlyFeeAmount.Value
            : await GetSuggestedMonthlyFeeAsync(dto.ClassId, cancellationToken);
        if (dto.CreateInitialMonthlyFee && dto.ClassId.HasValue)
        {
            var academicYear = await db.Classes.Where(c => c.Id == dto.ClassId).Select(c => c.AcademicYear).SingleAsync(cancellationToken);
            db.MonthlyFees.Add(new MonthlyFee
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Year = now.Year,
                Month = now.Month,
                Amount = feeAmount,
                AcademicYear = academicYear
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return new CreateStudentResultDto(student.Id, feeAmount, now.Year, now.Month);
    }

    public async Task UpdateStudentAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        student.DateOfBirth = dto.DateOfBirth;
        student.PlaceOfBirth = dto.PlaceOfBirth?.Trim();
        student.Gender = dto.Gender;
        student.PhotoUrl = dto.PhotoUrl;
        student.ClassId = dto.ClassId;
        student.Status = dto.Status;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.ParentChildren.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.MonthlyFees.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.Payments.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.AttendanceRecords.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.ExamResults.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.StudentPromotions.Where(x => x.StudentId == id).ExecuteDeleteAsync(cancellationToken);
        await db.GraduatedStudents.Where(x => x.OriginalStudentId == id).ExecuteDeleteAsync(cancellationToken);

        db.Students.Remove(student);
        await db.SaveChangesAsync(cancellationToken);

        var user = await userManager.FindByIdAsync(student.UserId.ToString());
        if (user is not null)
        {
            var deleted = await userManager.DeleteAsync(user);
            if (!deleted.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", deleted.Errors.Select(e => e.Description)));
            }
        }

        await tx.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParentListItemDto>> ListParentsAsync(CancellationToken cancellationToken = default)
    {
        return await (
            from p in db.Parents.AsNoTracking()
            join u in db.Users.AsNoTracking() on p.UserId equals u.Id
            orderby p.LastName, p.FirstName
            select new ParentListItemDto(
                p.Id,
                p.UserId,
                p.FirstName,
                p.LastName,
                p.Phone,
                p.PhotoUrl,
                u.Email,
                p.Children.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ParentDetailsDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parent = await (
            from p in db.Parents.AsNoTracking()
            join u in db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.Id == id
            select new
            {
                p.Id,
                p.UserId,
                p.FirstName,
                p.LastName,
                p.Phone,
                p.PhotoUrl,
                u.Email
            }).SingleOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            return null;
        }

        var children = await db.ParentChildren.AsNoTracking()
            .Where(l => l.ParentId == id)
            .Select(l => new LinkedStudentSummaryDto(
                l.Student.Id,
                l.Student.StudentNumber,
                l.Student.FirstName,
                l.Student.LastName,
                l.Student.Class != null ? l.Student.Class.Name : null))
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);

        return new ParentDetailsDto(
            parent.Id,
            parent.UserId,
            parent.FirstName,
            parent.LastName,
            parent.Phone,
            parent.PhotoUrl,
            parent.Email,
            children);
    }

    public async Task<Guid> CreateParentAsync(CreateParentDto dto, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var create = await userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, RoleNames.Parent);

        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Phone = dto.Phone,
            PhotoUrl = dto.PhotoUrl,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Parents.Add(parent);
        await db.SaveChangesAsync(cancellationToken);

        if (dto.StudentIds is { Length: > 0 })
        {
            db.ParentChildren.AddRange(dto.StudentIds.Distinct().Select(studentId => new ParentChild
            {
                ParentId = parent.Id,
                StudentId = studentId
            }));
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return parent.Id;
    }

    public async Task UpdateParentAsync(Guid id, UpdateParentDto dto, CancellationToken cancellationToken = default)
    {
        var parent = await db.Parents.SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Parent not found.");

        parent.FirstName = dto.FirstName.Trim();
        parent.LastName = dto.LastName.Trim();
        parent.Phone = dto.Phone;
        parent.PhotoUrl = dto.PhotoUrl;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parent = await db.Parents.SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Parent not found.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.ParentChildren.Where(x => x.ParentId == id).ExecuteDeleteAsync(cancellationToken);
        db.Parents.Remove(parent);
        await db.SaveChangesAsync(cancellationToken);

        var user = await userManager.FindByIdAsync(parent.UserId.ToString());
        if (user is not null)
        {
            var deleted = await userManager.DeleteAsync(user);
            if (!deleted.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", deleted.Errors.Select(e => e.Description)));
            }
        }

        await tx.CommitAsync(cancellationToken);
    }

    public async Task LinkParentChildAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var exists = await db.ParentChildren.AnyAsync(x => x.ParentId == parentId && x.StudentId == studentId, cancellationToken);
        if (exists)
        {
            return;
        }

        db.ParentChildren.Add(new ParentChild { ParentId = parentId, StudentId = studentId });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlinkParentChildAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var link = await db.ParentChildren.SingleOrDefaultAsync(x => x.ParentId == parentId && x.StudentId == studentId, cancellationToken);
        if (link is null)
        {
            return;
        }

        db.ParentChildren.Remove(link);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherListItemDto>> ListTeachersAsync(CancellationToken cancellationToken = default)
    {
        return await (
            from t in db.Teachers.AsNoTracking()
            join u in db.Users.AsNoTracking() on t.UserId equals u.Id
            orderby t.LastName, t.FirstName
            select new TeacherListItemDto(
                t.Id,
                t.UserId,
                t.EmployeeNumber,
                t.FirstName,
                t.LastName,
                t.PhotoUrl,
                u.Email,
                t.Assignments.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<TeacherDetailsDto?> GetTeacherAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await (
            from t in db.Teachers.AsNoTracking()
            join u in db.Users.AsNoTracking() on t.UserId equals u.Id
            where t.Id == id
            select new
            {
                t.Id,
                t.UserId,
                t.EmployeeNumber,
                t.FirstName,
                t.LastName,
                t.PhotoUrl,
                u.Email
            }).SingleOrDefaultAsync(cancellationToken);

        if (teacher is null)
        {
            return null;
        }

        var assignments = await db.TeacherSubjects.AsNoTracking()
            .Where(a => a.TeacherId == id)
            .Select(a => new TeacherAssignmentSummaryDto(
                a.SubjectId,
                a.Subject.Name,
                a.ClassId,
                a.Class.Name))
            .OrderBy(a => a.ClassName)
            .ThenBy(a => a.SubjectName)
            .ToListAsync(cancellationToken);

        return new TeacherDetailsDto(
            teacher.Id,
            teacher.UserId,
            teacher.EmployeeNumber,
            teacher.FirstName,
            teacher.LastName,
            teacher.PhotoUrl,
            teacher.Email,
            assignments);
    }

    public async Task<Guid> CreateTeacherAsync(CreateTeacherDto dto, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var create = await userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, RoleNames.Teacher);

        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmployeeNumber = dto.EmployeeNumber.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhotoUrl = dto.PhotoUrl,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Teachers.Add(teacher);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return teacher.Id;
    }

    public async Task UpdateTeacherAsync(Guid id, UpdateTeacherDto dto, CancellationToken cancellationToken = default)
    {
        var teacher = await db.Teachers.SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        teacher.EmployeeNumber = dto.EmployeeNumber.Trim();
        teacher.FirstName = dto.FirstName.Trim();
        teacher.LastName = dto.LastName.Trim();
        teacher.PhotoUrl = dto.PhotoUrl;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await db.Teachers.SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        var hasAcademicRecords = await db.AttendanceRecords.AnyAsync(x => x.RecordedByTeacherId == id, cancellationToken)
            || await db.ExamResults.AnyAsync(x => x.TeacherId == id, cancellationToken);
        if (hasAcademicRecords)
        {
            throw new InvalidOperationException("Cannot delete a teacher with attendance or exam records. Remove old records first or keep the profile archived.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.TeacherSubjects.Where(x => x.TeacherId == id).ExecuteDeleteAsync(cancellationToken);
        db.Teachers.Remove(teacher);
        await db.SaveChangesAsync(cancellationToken);

        var user = await userManager.FindByIdAsync(teacher.UserId.ToString());
        if (user is not null)
        {
            var deleted = await userManager.DeleteAsync(user);
            if (!deleted.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", deleted.Errors.Select(e => e.Description)));
            }
        }

        await tx.CommitAsync(cancellationToken);
    }
}
