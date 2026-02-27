using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAnnouncementByIdQuery : IRequest<AnnouncementDto?>
{
    public Guid Id { get; set; }

    public GetAnnouncementByIdQuery(Guid id)
    {
        Id = id;
    }
}
