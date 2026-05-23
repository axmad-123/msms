using MSMS.Application.DTOs.Common;
using MSMS.Application.DTOs.People;

namespace MSMS.Application.Abstractions;

public interface IPeopleAdminService
{
    Task<PagedResult<StudentListItemDto>> ListStudentsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StudentDetailsDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreateStudentResultDto> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);
    Task UpdateStudentAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default);
    Task DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetSuggestedMonthlyFeeAsync(Guid? classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParentListItemDto>> ListParentsAsync(CancellationToken cancellationToken = default);
    Task<ParentDetailsDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateParentAsync(CreateParentDto dto, CancellationToken cancellationToken = default);
    Task UpdateParentAsync(Guid id, UpdateParentDto dto, CancellationToken cancellationToken = default);
    Task DeleteParentAsync(Guid id, CancellationToken cancellationToken = default);
    Task LinkParentChildAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken = default);
    Task UnlinkParentChildAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherListItemDto>> ListTeachersAsync(CancellationToken cancellationToken = default);
    Task<TeacherDetailsDto?> GetTeacherAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateTeacherAsync(CreateTeacherDto dto, CancellationToken cancellationToken = default);
    Task UpdateTeacherAsync(Guid id, UpdateTeacherDto dto, CancellationToken cancellationToken = default);
    Task DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default);
}
