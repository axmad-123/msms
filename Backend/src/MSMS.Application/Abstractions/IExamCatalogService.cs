using MSMS.Application.DTOs.Exams;

namespace MSMS.Application.Abstractions;

public interface IExamCatalogService
{
    Task<IReadOnlyList<ExamDto>> ListAsync(string? academicYear, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateExamDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CreateExamDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
