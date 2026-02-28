namespace EmployeeService.Domain.Entities;

public class EmployeeGoal
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GoalType { get; set; } = string.Empty; // Individual, Team, Company
    public string Category { get; set; } = string.Empty; // Performance, Development, Project
    
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    public int Progress { get; set; } = 0; // 0-100%
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    
    public string? Metrics { get; set; } // How to measure success
    public string? Notes { get; set; }
    public Guid? ReviewerId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
