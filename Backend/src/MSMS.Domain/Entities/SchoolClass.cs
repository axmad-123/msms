using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("classes")]
public class SchoolClass
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string GradeLevel { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? Section { get; set; }

    [Required]
    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;

    public SchoolSection SchoolSection { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
}
