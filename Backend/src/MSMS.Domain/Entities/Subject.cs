using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("subjects")]
public class Subject
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
