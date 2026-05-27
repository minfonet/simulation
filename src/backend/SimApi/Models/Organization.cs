using System.ComponentModel.DataAnnotations;

namespace SimApi.Models;

public class Organization
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<SimulationSession> Sessions { get; set; } = new List<SimulationSession>();
}
