namespace EmployeeService.Domain.Entities;

public class EmployeeSalaryHistory
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal PreviousSalary { get; set; }
    public decimal NewSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string ChangeReason { get; set; } = string.Empty; // Promotion, Annual Increase, Performance Bonus, Market Adjustment
    public string? ApprovedBy { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
