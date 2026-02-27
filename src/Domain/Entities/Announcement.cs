namespace EmployeeService.Domain.Entities;

public class Announcement
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // General, Policy, Event, Emergency
    public bool IsPinned { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? DepartmentId { get; set; } // null = all departments
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department? Department { get; set; }
    public Employee? CreatedByEmployee { get; set; }
}
