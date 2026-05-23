using System.ComponentModel.DataAnnotations.Schema;

namespace MSMS.Domain.Entities;

[Table("parent_children")]
public class ParentChild
{
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }

    public Parent Parent { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
