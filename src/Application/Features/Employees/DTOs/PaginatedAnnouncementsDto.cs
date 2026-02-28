namespace EmployeeService.Application.Features.Employees.DTOs;

public class PaginatedAnnouncementsDto
{
    public IEnumerable<AnnouncementDto> Items { get; set; } = new List<AnnouncementDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
