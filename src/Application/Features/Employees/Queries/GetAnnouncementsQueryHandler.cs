using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, PaginatedAnnouncementsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnouncementsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedAnnouncementsDto> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Announcements.GetAllAsync();
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = await _unitOfWork.Departments.GetAllAsync();

        var query = all.AsEnumerable();

        if (!request.IncludeExpired)
            query = query.Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(a => a.Category == request.Category);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.DepartmentId == null || a.DepartmentId == request.DepartmentId);

        var totalCount = query.Count();

        var announcements = query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a =>
            {
                var creator = employees.FirstOrDefault(e => e.Id == a.CreatedBy);
                var dept = a.DepartmentId.HasValue ? departments.FirstOrDefault(d => d.Id == a.DepartmentId) : null;
                return new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    Category = a.Category,
                    IsPinned = a.IsPinned,
                    ExpiresAt = a.ExpiresAt,
                    DepartmentId = a.DepartmentId,
                    DepartmentName = dept?.Name,
                    CreatedBy = a.CreatedBy,
                    CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : null,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                };
            });

        return new PaginatedAnnouncementsDto
        {
            Items = announcements,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
