using MSMS.Application.DTOs.Parent;

namespace MSMS.Application.Abstractions;

public interface IParentPortalService
{
    Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsAsync(CancellationToken cancellationToken = default);
}
