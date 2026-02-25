using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateTeamCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DepartmentId { get; set; }
    public string? LeaderId { get; set; }
}
