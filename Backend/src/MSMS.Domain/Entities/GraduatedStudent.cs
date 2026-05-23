using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("graduated_students")]
public class GraduatedStudent
{
    public Guid Id { get; set; }

    public Guid OriginalStudentId { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly GraduationDate { get; set; }

    public Guid FinalClassId { get; set; }

    [Required]
    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;

    public StudentStatus FinalStatus { get; set; } = StudentStatus.Graduated;

    public DateTime ArchivedAtUtc { get; set; }

    public SchoolClass FinalClass { get; set; } = null!;
}
