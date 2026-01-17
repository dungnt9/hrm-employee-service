using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteEmployeeCommand : IRequest<bool>
{
    public Guid Id { get; }

    public DeleteEmployeeCommand(Guid id)
    {
        Id = id;
    }
}
