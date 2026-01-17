namespace EmployeeService.Domain.Entities;

public class EmployeeRole
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }

    public Employee Employee { get; set; } = null!;
}
