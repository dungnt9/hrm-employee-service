using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteDocumentCommand : IRequest<bool>
{
    public Guid DocumentId { get; set; }
    public Guid EmployeeId { get; set; }
}
