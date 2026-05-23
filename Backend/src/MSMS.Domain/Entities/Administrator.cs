using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("admins")]
public class Administrator
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
