using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetTeamByIdQuery : IRequest<TeamDto?>
{
    public Guid Id { get; set; }
    public GetTeamByIdQuery(Guid id) => Id = id;
}
