namespace EmployeeService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }

    public Employee? PerformedByEmployee { get; set; }
}
