using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(request.Id);
        if (team == null)
            return false;

        await _unitOfWork.Teams.DeleteAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
