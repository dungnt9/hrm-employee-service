using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(request.Id);
        if (team == null)
            return false;

        team.Name = request.Name;
        team.Description = request.Description;
        if (Guid.TryParse(request.DepartmentId, out var departmentId))
            team.DepartmentId = departmentId;
        team.LeaderId = Guid.TryParse(request.LeaderId, out var leaderId) ? leaderId : team.LeaderId;
        team.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Teams.UpdateAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
