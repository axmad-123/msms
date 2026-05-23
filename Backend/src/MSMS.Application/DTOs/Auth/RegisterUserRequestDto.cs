namespace MSMS.Application.DTOs.Auth;

public sealed class RegisterUserRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public string? StudentNumber { get; set; }
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public DateOnly? StudentDateOfBirth { get; set; }
    public Guid? StudentClassId { get; set; }

    public string? ParentFirstName { get; set; }
    public string? ParentLastName { get; set; }
    public string? ParentPhone { get; set; }

    public string? TeacherEmployeeNumber { get; set; }
    public string? TeacherFirstName { get; set; }
    public string? TeacherLastName { get; set; }

    public string? AdminDisplayName { get; set; }
}
