using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetDepartmentByIdQuery : IRequest<DepartmentDto?>
{
    public Guid Id { get; set; }
    public GetDepartmentByIdQuery(Guid id) => Id = id;
}
