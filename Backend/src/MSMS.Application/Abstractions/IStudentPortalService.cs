using MSMS.Application.DTOs.Exams;
using MSMS.Application.DTOs.Student;

namespace MSMS.Application.Abstractions;

public interface IStudentPortalService
{
    Task<StudentPortalDto> GetMyPortalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResultDto>> GetMyExamResultsAsync(CancellationToken cancellationToken = default);
}
