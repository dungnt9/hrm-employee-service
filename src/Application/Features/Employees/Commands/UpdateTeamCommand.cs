using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateTeamCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DepartmentId { get; set; }
    public string? LeaderId { get; set; }
}
