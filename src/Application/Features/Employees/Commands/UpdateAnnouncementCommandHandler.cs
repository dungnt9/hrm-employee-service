using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateAnnouncementCommandHandler : IRequestHandler<UpdateAnnouncementCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnnouncementCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(request.Id);
        if (announcement == null)
            return false;

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.Category = request.Category;
        announcement.IsPinned = request.IsPinned;
        announcement.ExpiresAt = request.ExpiresAt;
        announcement.DepartmentId = request.DepartmentId;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Announcements.UpdateAsync(announcement);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
