using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("monthly_fees")]
public class MonthlyFee
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;

    public Student Student { get; set; } = null!;
}
