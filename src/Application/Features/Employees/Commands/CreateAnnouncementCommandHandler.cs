using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnnouncementCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Category = request.Category,
            IsPinned = request.IsPinned,
            ExpiresAt = request.ExpiresAt,
            DepartmentId = request.DepartmentId,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Announcements.AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync();
        return announcement.Id;
    }
}
