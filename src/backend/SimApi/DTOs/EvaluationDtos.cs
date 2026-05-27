namespace SimApi.DTOs;

public class EvaluationResponse
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public double Score { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}
