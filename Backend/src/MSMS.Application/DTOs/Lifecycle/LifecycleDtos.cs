namespace MSMS.Application.DTOs.Lifecycle;

public sealed record PromoteStudentDto(Guid StudentId, Guid ToClassId, DateOnly PromotionDate, string AcademicYear);

public sealed record GraduateStudentDto(Guid StudentId, DateOnly GraduationDate, string AcademicYear);

public sealed record AutoPromoteRequestDto(string NewAcademicYear, DateOnly PromotionDate);

public sealed record AutoPromoteResultDto(int PromotedCount, int GraduatedCount, IReadOnlyList<string> Messages);

public sealed record GraduatedStudentDto(
    Guid Id,
    Guid OriginalStudentId,
    string StudentNumber,
    string FullName,
    DateOnly GraduationDate,
    Guid FinalClassId,
    string AcademicYear,
    DateTime ArchivedAtUtc);
