using MSMS.Application.DTOs.People;
using MSMS.Application.DTOs.School;

namespace MSMS.Application.Abstractions;

public interface ITeacherPortalService
{
    Task<IReadOnlyList<TeacherSubjectDto>> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentListItemDto>> GetClassStudentsAsync(Guid classId, CancellationToken cancellationToken = default);
}
