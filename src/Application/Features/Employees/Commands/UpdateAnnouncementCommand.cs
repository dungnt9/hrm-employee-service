using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateAnnouncementCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsPinned { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? DepartmentId { get; set; }
}
