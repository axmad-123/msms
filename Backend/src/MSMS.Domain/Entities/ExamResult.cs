using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("exam_results")]
public class ExamResult
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid SubjectId { get; set; }

    public Guid TeacherId { get; set; }

    public ExamType ExamType { get; set; }

    public decimal Marks { get; set; }

    public decimal MaxMarks { get; set; } = 100;

    [Required]
    [MaxLength(16)]
    public string Grade { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;

    public DateTime EnteredAtUtc { get; set; }

    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
}
