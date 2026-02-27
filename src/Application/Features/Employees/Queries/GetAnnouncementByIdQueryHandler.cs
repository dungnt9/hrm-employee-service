using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAnnouncementByIdQueryHandler : IRequestHandler<GetAnnouncementByIdQuery, AnnouncementDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnouncementByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnnouncementDto?> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _unitOfWork.Announcements.GetByIdAsync(request.Id);
        if (a == null) return null;

        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = await _unitOfWork.Departments.GetAllAsync();

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
    }
}
