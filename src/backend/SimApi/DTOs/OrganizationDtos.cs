using System.ComponentModel.DataAnnotations;

namespace SimApi.DTOs;

public class CreateOrganizationRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

public class OrganizationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}
