using MSMS.Application.DTOs.Lifecycle;

namespace MSMS.Application.Abstractions;

public interface IStudentLifecycleService
{
    Task PromoteAsync(PromoteStudentDto dto, CancellationToken cancellationToken = default);
    Task GraduateAsync(GraduateStudentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GraduatedStudentDto>> ListGraduatedAsync(CancellationToken cancellationToken = default);
    Task<AutoPromoteResultDto> AutoPromoteAllAsync(AutoPromoteRequestDto dto, CancellationToken cancellationToken = default);
}
