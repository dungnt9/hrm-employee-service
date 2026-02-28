namespace EmployeeService.Domain.Entities;

public class EmployeePerformanceReview
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewType { get; set; } = string.Empty; // Annual, Mid-Year, Probation, 360-Degree
    public DateTime ReviewPeriodStart { get; set; }
    public DateTime ReviewPeriodEnd { get; set; }
    public DateTime ReviewDate { get; set; }
    
    // Ratings (1-5 scale)
    public int? PerformanceRating { get; set; }
    public int? BehaviorRating { get; set; }
    public int? TeamworkRating { get; set; }
    public int? InitiativeRating { get; set; }
    public int? OverallRating { get; set; }
    
    // Feedback
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? Goals { get; set; }
    public string? ReviewerComments { get; set; }
    public string? EmployeeComments { get; set; }
    
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Acknowledged, Completed
    public DateTime? EmployeeAcknowledgedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
