namespace EmployeeService.Domain.Entities;

public class EmployeeOnboarding
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
    
    // Checklist items (JSON or separate table)
    public string? PreBoardingTasks { get; set; } // JSON: [{"task": "Send welcome email", "completed": true, "completedAt": "..."}]
    public string? FirstDayTasks { get; set; }
    public string? FirstWeekTasks { get; set; }
    public string? FirstMonthTasks { get; set; }
    
    public Guid? BuddyId { get; set; } // Assigned buddy/mentor
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
