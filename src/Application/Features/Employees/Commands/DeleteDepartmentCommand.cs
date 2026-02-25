using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteDepartmentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
