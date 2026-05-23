using MSMS.Domain.Enums;

namespace MSMS.Application.DTOs.School;

public sealed record ClassDto(Guid Id, string Name, string GradeLevel, string? Section, string AcademicYear, SchoolSection SchoolSection);

public sealed record ClassStudentSummaryDto(Guid Id, string StudentNumber, string FirstName, string LastName, string? PhotoUrl);

public sealed record ClassSubjectSummaryDto(Guid SubjectId, string SubjectName, Guid TeacherId, string TeacherName);

public sealed record ClassDetailsDto(
    Guid Id,
    string Name,
    string GradeLevel,
    string? Section,
    string AcademicYear,
    SchoolSection SchoolSection,
    int StudentCount,
    int SubjectCount,
    IReadOnlyList<ClassStudentSummaryDto> Students,
    IReadOnlyList<ClassSubjectSummaryDto> Subjects);

public sealed record CreateClassDto(string Name, string GradeLevel, string? Section, string AcademicYear);

public sealed record SubjectDto(Guid Id, string Name, string Code);

public sealed record CreateSubjectDto(string Name, string Code);

public sealed record TeacherSubjectAssignmentDto(Guid TeacherId, Guid SubjectId, Guid ClassId);

public sealed record TeacherSubjectDto(Guid TeacherId, string TeacherName, Guid SubjectId, string SubjectName, Guid ClassId, string ClassName);
