using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("attendance_records")]
public class AttendanceRecord
{
    public Guid Id { get; set; }

    public Guid AttendanceSessionId { get; set; }

    public Guid StudentId { get; set; }

    public Guid RecordedByTeacherId { get; set; }

    public AttendanceMark Mark { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public AttendanceSession Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Teacher RecordedByTeacher { get; set; } = null!;
}
