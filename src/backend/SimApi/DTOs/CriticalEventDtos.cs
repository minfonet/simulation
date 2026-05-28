namespace SimApi.DTOs;

public class CriticalEventResponse
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
