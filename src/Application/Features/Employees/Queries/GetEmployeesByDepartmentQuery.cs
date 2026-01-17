using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetEmployeesByDepartmentQuery : IRequest<IEnumerable<EmployeeDto>>
{
    public Guid DepartmentId { get; }

    public GetEmployeesByDepartmentQuery(Guid departmentId)
    {
        DepartmentId = departmentId;
    }
}
