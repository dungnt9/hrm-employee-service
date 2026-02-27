using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteAnnouncementCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
