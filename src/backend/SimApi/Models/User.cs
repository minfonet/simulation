using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public Guid OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [JsonIgnore]
    public Organization? Organization { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }

    [JsonIgnore]
    public ICollection<SimulationSession> InstructorSessions { get; set; } = new List<SimulationSession>();

    [JsonIgnore]
    public ICollection<SimulationSession> TraineeSessions { get; set; } = new List<SimulationSession>();
}
