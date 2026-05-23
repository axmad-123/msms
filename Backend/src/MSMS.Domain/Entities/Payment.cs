using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MSMS.Domain.Enums;

namespace MSMS.Domain.Entities;

[Table("payments")]
public class Payment
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Amount { get; set; }

    public DateOnly PaymentDate { get; set; }

    public PaymentStatus Status { get; set; }

    public Guid RecordedByAdminId { get; set; }

    [MaxLength(512)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Student Student { get; set; } = null!;
    public Administrator RecordedByAdmin { get; set; } = null!;
}
