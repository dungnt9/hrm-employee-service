using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteContactCommand : IRequest<bool>
{
    public Guid ContactId { get; set; }
    public Guid EmployeeId { get; set; }
}
