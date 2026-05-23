using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("teachers")]
public class Teacher
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? PhotoUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<TeacherSubject> Assignments { get; set; } = new List<TeacherSubject>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
