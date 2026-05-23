using MSMS.Domain.Enums;

namespace MSMS.Application.DTOs.People;

public sealed record StudentListItemDto(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    Guid? ClassId,
    StudentStatus Status,
    string? PhotoUrl,
    Gender? Gender,
    string? ClassName,
    string? GradeLevel);

public sealed record StudentDetailsDto(
    Guid Id,
    Guid UserId,
    string StudentNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? PlaceOfBirth,
    Gender? Gender,
    string? PhotoUrl,
    Guid? ClassId,
    StudentStatus Status,
    string? ClassName,
    string? GradeLevel,
    SchoolSection? SchoolSection,
    string? Email);

public sealed record CreateStudentDto(
    string Email,
    string Password,
    string FullName,
    string StudentNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? PlaceOfBirth,
    Gender? Gender,
    string? PhotoUrl,
    Guid? ClassId,
    decimal? MonthlyFeeAmount,
    bool CreateInitialMonthlyFee = true);

public sealed record CreateStudentResultDto(
    Guid StudentId,
    decimal MonthlyFeeAmount,
    int FeeYear,
    int FeeMonth);

public sealed record UpdateStudentDto(
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? PlaceOfBirth,
    Gender? Gender,
    string? PhotoUrl,
    Guid? ClassId,
    StudentStatus Status);

public sealed record LinkedStudentSummaryDto(Guid Id, string StudentNumber, string FirstName, string LastName, string? ClassName);

public sealed record ParentListItemDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string? PhotoUrl,
    string? Email,
    int LinkedStudentCount);

public sealed record ParentDetailsDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string? PhotoUrl,
    string? Email,
    IReadOnlyList<LinkedStudentSummaryDto> Children);

public sealed record CreateParentDto(
    string Email,
    string Password,
    string FullName,
    string FirstName,
    string LastName,
    string? Phone,
    string? PhotoUrl,
    Guid[]? StudentIds);

public sealed record UpdateParentDto(
    string FirstName,
    string LastName,
    string? Phone,
    string? PhotoUrl);

public sealed record TeacherAssignmentSummaryDto(Guid SubjectId, string SubjectName, Guid ClassId, string ClassName);

public sealed record TeacherListItemDto(
    Guid Id,
    Guid UserId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? PhotoUrl,
    string? Email,
    int AssignmentCount);

public sealed record TeacherDetailsDto(
    Guid Id,
    Guid UserId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? PhotoUrl,
    string? Email,
    IReadOnlyList<TeacherAssignmentSummaryDto> Assignments);

public sealed record CreateTeacherDto(
    string Email,
    string Password,
    string FullName,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? PhotoUrl);

public sealed record UpdateTeacherDto(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? PhotoUrl);
