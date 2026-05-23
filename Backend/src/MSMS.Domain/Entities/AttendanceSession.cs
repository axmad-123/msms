using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("attendance_sessions")]
public class AttendanceSession
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public DateOnly SessionDate { get; set; }

    /// <summary>1 = first session of the day, 2 = second session.</summary>
    public int SessionNumber { get; set; }

    public Guid StartedByUserId { get; set; }

    public AttendanceSessionStatus Status { get; set; } = AttendanceSessionStatus.Open;

    public DateTime CreatedAtUtc { get; set; }

    public SchoolClass Class { get; set; } = null!;
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}
