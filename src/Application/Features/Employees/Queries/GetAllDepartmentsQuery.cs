using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAllDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>
{
    public Guid? CompanyId { get; set; }
}
