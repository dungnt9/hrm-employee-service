using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetEmployeeByIdQuery : IRequest<EmployeeDto?>
{
    public Guid Id { get; set; }

    public GetEmployeeByIdQuery(Guid id)
    {
        Id = id;
    }
}
