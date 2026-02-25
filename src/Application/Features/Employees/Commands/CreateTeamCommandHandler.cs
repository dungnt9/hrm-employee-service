using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Name.Replace(" ", "").ToUpper()[..Math.Min(10, request.Name.Replace(" ", "").Length)],
            Description = request.Description,
            DepartmentId = request.DepartmentId,
            LeaderId = Guid.TryParse(request.LeaderId, out var leaderId) ? leaderId : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.AddAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return team.Id;
    }
}
