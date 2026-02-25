using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateDepartmentCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ManagerId { get; set; }
}
