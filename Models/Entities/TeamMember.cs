using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.Models.Entities;

[Index(nameof(TeamId), nameof(UserId), IsUnique = true)]
public class TeamMember
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("TeamId")]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
