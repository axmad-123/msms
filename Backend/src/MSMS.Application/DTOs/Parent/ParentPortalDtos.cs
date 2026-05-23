namespace MSMS.Application.DTOs.Parent;

public sealed record LinkedStudentDto(Guid Id, string StudentNumber, string FirstName, string LastName, Guid? ClassId);
