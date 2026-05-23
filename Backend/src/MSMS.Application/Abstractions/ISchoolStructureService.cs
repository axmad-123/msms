using MSMS.Application.DTOs.School;

namespace MSMS.Application.Abstractions;

public interface ISchoolStructureService
{
    Task<IReadOnlyList<ClassDto>> ListClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassDetailsDto?> GetClassAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateClassAsync(CreateClassDto dto, CancellationToken cancellationToken = default);
    Task UpdateClassAsync(Guid id, CreateClassDto dto, CancellationToken cancellationToken = default);
    Task DeleteClassAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubjectDto>> ListSubjectsAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateSubjectAsync(CreateSubjectDto dto, CancellationToken cancellationToken = default);
    Task UpdateSubjectAsync(Guid id, CreateSubjectDto dto, CancellationToken cancellationToken = default);
    Task DeleteSubjectAsync(Guid id, CancellationToken cancellationToken = default);

    Task AssignTeacherAsync(TeacherSubjectAssignmentDto dto, CancellationToken cancellationToken = default);
    Task UnassignTeacherAsync(Guid teacherId, Guid subjectId, Guid classId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherSubjectDto>> ListAssignmentsAsync(Guid? teacherId, CancellationToken cancellationToken = default);
}
