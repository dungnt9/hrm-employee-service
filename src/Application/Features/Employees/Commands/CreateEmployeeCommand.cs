using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<Guid>
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public string EmployeeType { get; set; } = "FullTime";
}
