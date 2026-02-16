using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.Models.Entities;

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "ToDo"; // ToDo, InProgress, Done

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    public int AssignedToId { get; set; }

    [Required]
    public int CreatedById { get; set; }

    public int? TeamId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("AssignedToId")]
    public virtual User AssignedTo { get; set; } = null!;

    [ForeignKey("CreatedById")]
    public virtual User CreatedBy { get; set; } = null!;

    [ForeignKey("TeamId")]
    public virtual Team? Team { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
