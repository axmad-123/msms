using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("students")]
public class Student
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(256)]
    public string? PlaceOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [MaxLength(512)]
    public string? PhotoUrl { get; set; }

    public Guid? ClassId { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateTime CreatedAtUtc { get; set; }

    public SchoolClass? Class { get; set; }
    public ICollection<ParentChild> ParentLinks { get; set; } = new List<ParentChild>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    public ICollection<MonthlyFee> MonthlyFees { get; set; } = new List<MonthlyFee>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<StudentPromotion> Promotions { get; set; } = new List<StudentPromotion>();
}
