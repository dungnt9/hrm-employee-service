using AutoMapper;
using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, IEnumerable<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTeamsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeamDto>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = await _unitOfWork.Teams.GetAllAsync();

        var teams = query.ToList();

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            teams = teams.Where(t => t.DepartmentId == request.DepartmentId.Value).ToList();
        }

        var teamDtos = new List<TeamDto>();

        foreach (var team in teams)
        {
            var dto = new TeamDto
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
            teamDtos.Add(dto);
        }

        return teamDtos;
    }
}
