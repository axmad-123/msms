using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.People;
using MSMS.Application.DTOs.School;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class TeacherPortalService(ApplicationDbContext db, ISchoolStructureService school, ICurrentUserAccessor currentUser) : ITeacherPortalService
{
    public async Task<IReadOnlyList<TeacherSubjectDto>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var teacherId = await db.Teachers.Where(t => t.UserId == userId).Select(t => t.Id).SingleAsync(cancellationToken);
        return await school.ListAssignmentsAsync(teacherId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentListItemDto>> GetClassStudentsAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var teacherId = await db.Teachers.Where(t => t.UserId == userId).Select(t => t.Id).SingleAsync(cancellationToken);
        var assigned = await db.TeacherSubjects.AsNoTracking()
            .AnyAsync(a => a.TeacherId == teacherId && a.ClassId == classId, cancellationToken);
        if (!assigned)
            throw new UnauthorizedAccessException("You are not assigned to this class.");

        return await db.Students.AsNoTracking()
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
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
    }
}
