using MSMS.Application.DTOs.Exams;

namespace MSMS.Application.Abstractions;

public interface IExamResultService
{
    Task<IReadOnlyList<ExamResultDto>> GetForStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResultDto>> GetForClassSubjectAsync(Guid classId, Guid subjectId, int? examType, CancellationToken cancellationToken = default);
    Task UpsertResultsAsync(UpsertExamResultsDto dto, CancellationToken cancellationToken = default);
}
