using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateEmployeeCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public string? Position { get; set; }
    public Guid? ManagerId { get; set; }
    public string Status { get; set; } = "Active";
}
