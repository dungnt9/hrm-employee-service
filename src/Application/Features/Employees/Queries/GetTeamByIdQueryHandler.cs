using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, TeamDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamDto?> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Teams.GetAllAsync();
        var team = all.FirstOrDefault(t => t.Id == request.Id);
        if (team == null) return null;

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Code = team.Code,
            Description = team.Description,
            DepartmentId = team.DepartmentId,
            DepartmentName = team.Department?.Name,
            LeaderId = team.LeaderId,
            LeaderName = team.Leader != null ? $"{team.Leader.FirstName} {team.Leader.LastName}" : null,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }
}
