using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetEmployeesByManagerQuery : IRequest<IEnumerable<EmployeeDto>>
{
    public Guid ManagerId { get; }

    public GetEmployeesByManagerQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}
