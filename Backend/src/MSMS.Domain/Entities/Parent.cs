using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("parents")]
public class Parent
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? PhotoUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ParentChild> Children { get; set; } = new List<ParentChild>();
}
