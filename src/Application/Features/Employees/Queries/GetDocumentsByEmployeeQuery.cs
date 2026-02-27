using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetDocumentsByEmployeeQuery : IRequest<IEnumerable<DocumentDto>>
{
    public Guid EmployeeId { get; set; }

    public GetDocumentsByEmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
