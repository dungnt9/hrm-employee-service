using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetContactsByEmployeeQuery : IRequest<IEnumerable<ContactDto>>
{
    public Guid EmployeeId { get; set; }

    public GetContactsByEmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
