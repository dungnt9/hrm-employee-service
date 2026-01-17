namespace EmployeeService.Application.Features.Employees.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? HireDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EmployeeType { get; set; } = string.Empty;
    public decimal? BaseSalary { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
