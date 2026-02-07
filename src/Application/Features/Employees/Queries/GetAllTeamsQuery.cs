using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAllTeamsQuery : IRequest<IEnumerable<TeamDto>>
{
    public Guid? DepartmentId { get; set; }
}
