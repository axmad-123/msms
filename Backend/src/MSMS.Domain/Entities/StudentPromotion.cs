using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("student_promotions")]
public class StudentPromotion
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid? FromClassId { get; set; }

    public Guid ToClassId { get; set; }

    public DateOnly PromotionDate { get; set; }

    [Required]
    [MaxLength(32)]
    public string AcademicYear { get; set; } = string.Empty;

    public Student Student { get; set; } = null!;
    public SchoolClass? FromClass { get; set; }
    public SchoolClass ToClass { get; set; } = null!;
}
