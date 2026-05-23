using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("teacher_subjects")]
public class TeacherSubject
{
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public SchoolClass Class { get; set; } = null!;
}
