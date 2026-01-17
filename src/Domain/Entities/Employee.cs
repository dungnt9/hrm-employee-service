using EmployeeService.Domain.Enums;

namespace EmployeeService.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string? IdentityNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public Guid? ManagerId { get; set; }
    public string? KeycloakUserId { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public EmployeeType EmployeeType { get; set; } = EmployeeType.FullTime;
    public decimal? BaseSalary { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? TaxCode { get; set; }
    public string? SocialInsuranceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public Department? Department { get; set; }
    public Team? Team { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeContact> EmergencyContacts { get; set; } = new List<EmployeeContact>();
}
