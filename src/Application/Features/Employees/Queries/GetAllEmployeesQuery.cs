using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>
{
}
