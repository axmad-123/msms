using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("exams")]
public class Exam
{
    public Guid Id { get; set; }

    public ExamType ExamType { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;
}
