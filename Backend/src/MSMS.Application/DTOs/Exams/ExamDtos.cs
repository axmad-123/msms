namespace MSMS.Application.DTOs.Exams;

public sealed record ExamDto(Guid Id, int ExamType, string Name, string AcademicYear);

public sealed record CreateExamDto(int ExamType, string Name, string AcademicYear);

public sealed record ExamResultDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid SubjectId,
    string SubjectName,
    int ExamType,
    decimal Marks,
    decimal MaxMarks,
    string Grade,
    string AcademicYear,
    DateTime EnteredAtUtc);

public sealed record UpsertExamResultsDto(
    Guid ClassId,
    Guid SubjectId,
    int ExamType,
    string AcademicYear,
    IReadOnlyList<ExamResultLineDto> Lines);

public sealed record ExamResultLineDto(Guid StudentId, decimal Marks, decimal MaxMarks, string Grade);
