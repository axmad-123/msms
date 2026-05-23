using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Application.DTOs.Student;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class StudentPortalService(ApplicationDbContext db, IExamResultService examResults, ICurrentUserAccessor currentUser) : IStudentPortalService
{
    public async Task<StudentPortalDto> GetMyPortalAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileAsync(cancellationToken);
        var results = await GetMyExamResultsAsync(cancellationToken);
        return new StudentPortalDto(profile, results);
    }

    public async Task<IReadOnlyList<ExamResultDto>> GetMyExamResultsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var studentId = await db.Students.Where(s => s.UserId == userId).Select(s => s.Id).SingleAsync(cancellationToken);
        return await examResults.GetForStudentAsync(studentId, cancellationToken);
    }

    private async Task<StudentProfileDto> GetProfileAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        return await db.Students.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new StudentProfileDto(
                s.Id,
                s.StudentNumber,
                s.FirstName,
                s.LastName,
                s.DateOfBirth,
                s.PlaceOfBirth,
                s.Gender,
                s.PhotoUrl,
                s.Class != null ? s.Class.Name : null,
                s.Class != null ? s.Class.GradeLevel : null,
                s.Class != null ? s.Class.SchoolSection : null,
                s.Class != null ? s.Class.AcademicYear : null,
                null))
            .SingleAsync(cancellationToken);
    }
}
