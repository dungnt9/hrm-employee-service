namespace EmployeeService.Application.Features.Employees.DTOs;

public class ContactDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimary { get; set; }
}
