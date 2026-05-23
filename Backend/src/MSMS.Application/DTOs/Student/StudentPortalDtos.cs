using MSMS.Application.DTOs.Exams;
using MSMS.Domain.Enums;

namespace MSMS.Application.DTOs.Student;

public sealed record StudentProfileDto(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? PlaceOfBirth,
    Gender? Gender,
    string? PhotoUrl,
    string? ClassName,
    string? GradeLevel,
    SchoolSection? SchoolSection,
    string? AcademicYear,
    string? Email);

public sealed record StudentPortalDto(
    StudentProfileDto Profile,
    IReadOnlyList<ExamResultDto> ExamResults);
