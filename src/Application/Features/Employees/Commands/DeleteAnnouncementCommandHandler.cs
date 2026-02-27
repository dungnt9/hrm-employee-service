using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnnouncementCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(request.Id);
        if (announcement == null)
            return false;

        await _unitOfWork.Announcements.DeleteAsync(announcement);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
