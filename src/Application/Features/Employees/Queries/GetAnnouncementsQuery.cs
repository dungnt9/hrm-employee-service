using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAnnouncementsQuery : IRequest<IEnumerable<AnnouncementDto>>
{
    public string? Category { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IncludeExpired { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
