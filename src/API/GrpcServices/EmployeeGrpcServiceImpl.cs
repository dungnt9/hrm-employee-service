using AutoMapper;
using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.Commands;
using EmployeeService.Application.Features.Employees.DTOs;
using EmployeeService.Application.Features.Employees.Queries;
using EmployeeService.Domain.Entities;
using EmployeeService.Protos;
using Grpc.Core;
using MediatR;

namespace EmployeeService.GrpcServices;

public class EmployeeGrpcServiceImpl : EmployeeGrpc.EmployeeGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeGrpcServiceImpl> _logger;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeGrpcServiceImpl(IMediator mediator, IMapper mapper, ILogger<EmployeeGrpcServiceImpl> logger, IEmployeeRepository employeeRepository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
        _employeeRepository = employeeRepository;
    }

    public override async Task<EmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
            {
                _logger.LogWarning("Invalid employee ID format: {EmployeeId}", request.EmployeeId);
                return new EmployeeResponse();
            }

            var query = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(query);

            if (employee == null)
            {
                _logger.LogInformation("Employee not found: {EmployeeId}", employeeId);
                return new EmployeeResponse();
            }

            return MapToResponse(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the employee."));
        }
    }

    public override async Task<EmployeesResponse> GetEmployees(GetEmployeesRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllEmployeesQuery();
            var employees = await _mediator.Send(query);

            var employeeList = employees.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
            {
                employeeList = employeeList.Where(e => e.DepartmentId == deptId).ToList();
            }

            if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
            {
                employeeList = employeeList.Where(e => e.TeamId == teamId).ToList();
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                employeeList = employeeList.Where(e =>
                    e.FirstName.ToLower().Contains(search) ||
                    e.LastName.ToLower().Contains(search) ||
                    e.Email.ToLower().Contains(search)).ToList();
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var statusFilter = request.Status.ToLower();
                employeeList = employeeList.Where(e =>
                    !string.IsNullOrEmpty(e.Status) && e.Status.ToLower() == statusFilter).ToList();
            }

            var totalCount = employeeList.Count;
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var paginatedEmployees = employeeList
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new EmployeesResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            response.Employees.AddRange(paginatedEmployees.Select(MapToResponse));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving employees."));
        }
    }

    public override async Task<EmployeeResponse> CreateEmployee(CreateEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateEmployeeCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                DepartmentId = string.IsNullOrEmpty(request.DepartmentId) ? null : Guid.Parse(request.DepartmentId),
                TeamId = string.IsNullOrEmpty(request.TeamId) ? null : Guid.Parse(request.TeamId),
                Position = request.Position,
                HireDate = string.IsNullOrEmpty(request.HireDate) ? null : DateTime.Parse(request.HireDate)
            };

            var employeeId = await _mediator.Send(command);
            var getQuery = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(getQuery);

            if (employee == null)
                return new EmployeeResponse();

            return MapToResponse(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the employee."));
        }
    }

    public override async Task<EmployeeResponse> UpdateEmployee(UpdateEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
            {
                _logger.LogWarning("Invalid employee ID format: {EmployeeId}", request.EmployeeId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID format."));
            }

            var command = new UpdateEmployeeCommand
            {
                Id = employeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone,
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : null,
                TeamId = Guid.TryParse(request.TeamId, out var teamId) ? teamId : null,
                Position = string.IsNullOrEmpty(request.Position) ? null : request.Position,
                ManagerId = Guid.TryParse(request.ManagerId, out var managerId) ? managerId : null,
                Status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status,
            };

            var success = await _mediator.Send(command);
            if (!success)
            {
                _logger.LogInformation("Employee not found for update: {EmployeeId}", employeeId);
                return new EmployeeResponse();
            }

            var getQuery = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(getQuery);
            if (employee == null)
                return new EmployeeResponse();

            return MapToResponse(employee);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the employee."));
        }
    }

    public override async Task<DepartmentsResponse> GetDepartments(GetDepartmentsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllDepartmentsQuery();

            if (!string.IsNullOrEmpty(request.CompanyId) && Guid.TryParse(request.CompanyId, out var companyId))
            {
                query.CompanyId = companyId;
            }

            var departments = await _mediator.Send(query);

            var response = new DepartmentsResponse();

            foreach (var dept in departments)
            {
                response.Departments.Add(new Protos.Department
                {
                    Id = dept.Id.ToString(),
                    Name = dept.Name,
                    CompanyId = dept.CompanyId.ToString(),
                    ManagerId = dept.ManagerId?.ToString() ?? string.Empty,
                    ManagerName = dept.ManagerName ?? string.Empty,
                    CreatedAt = dept.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = dept.Description ?? string.Empty
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving departments."));
        }
    }

    public override async Task<TeamsResponse> GetTeams(GetTeamsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllTeamsQuery();

            if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var departmentId))
            {
                query.DepartmentId = departmentId;
            }

            var teams = await _mediator.Send(query);

            var response = new TeamsResponse();

            foreach (var team in teams)
            {
                response.Teams.Add(new Protos.Team
                {
                    Id = team.Id.ToString(),
                    Name = team.Name,
                    DepartmentId = team.DepartmentId.ToString(),
                    ManagerId = team.LeaderId?.ToString() ?? string.Empty,
                    ManagerName = team.LeaderName ?? string.Empty,
                    CreatedAt = team.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = team.Description ?? string.Empty
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving teams."));
        }
    }

    public override async Task<EmployeeResponse> GetEmployeeByKeycloakId(GetEmployeeByKeycloakIdRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.KeycloakUserId))
                return new EmployeeResponse();

            var employee = await _employeeRepository.GetByKeycloakUserIdAsync(request.KeycloakUserId);
            if (employee == null)
                return new EmployeeResponse();

            var dto = _mapper.Map<EmployeeDto>(employee);
            return MapToResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by Keycloak ID {KeycloakUserId}", request.KeycloakUserId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the employee."));
        }
    }

    public override async Task<Protos.Department> GetDepartment(GetDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            if (dept == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Department not found."));

            return MapDepartmentToProto(dept);
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the department."));
        }
    }

    public override async Task<Protos.Department> CreateDepartment(CreateDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateDepartmentCommand
            {
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                ManagerId = string.IsNullOrEmpty(request.ManagerId) ? null : request.ManagerId
            };
            var id = await _mediator.Send(command);
            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            return dept != null ? MapDepartmentToProto(dept) : new Protos.Department();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the department."));
        }
    }

    public override async Task<Protos.Department> UpdateDepartment(UpdateDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var command = new UpdateDepartmentCommand
            {
                Id = id,
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                ManagerId = string.IsNullOrEmpty(request.ManagerId) ? null : request.ManagerId
            };
            var success = await _mediator.Send(command);
            if (!success)
                throw new RpcException(new Status(StatusCode.NotFound, "Department not found."));

            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            return dept != null ? MapDepartmentToProto(dept) : new Protos.Department();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the department."));
        }
    }

    public override async Task<DeleteDepartmentResponse> DeleteDepartment(DeleteDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var command = new DeleteDepartmentCommand { Id = id };
            var success = await _mediator.Send(command);
            return new DeleteDepartmentResponse
            {
                Success = success,
                Message = success ? "Department deleted." : "Department not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the department."));
        }
    }

    public override async Task<Protos.Team> GetTeam(GetTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            if (team == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found."));

            return MapTeamToProto(team);
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the team."));
        }
    }

    public override async Task<Protos.Team> CreateTeam(CreateTeamRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateTeamCommand
            {
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : Guid.Empty,
                LeaderId = string.IsNullOrEmpty(request.LeaderId) ? null : request.LeaderId
            };
            var id = await _mediator.Send(command);
            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            return team != null ? MapTeamToProto(team) : new Protos.Team();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the team."));
        }
    }

    public override async Task<Protos.Team> UpdateTeam(UpdateTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var command = new UpdateTeamCommand
            {
                Id = id,
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                DepartmentId = string.IsNullOrEmpty(request.DepartmentId) ? null : request.DepartmentId,
                LeaderId = string.IsNullOrEmpty(request.LeaderId) ? null : request.LeaderId
            };
            var success = await _mediator.Send(command);
            if (!success)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found."));

            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            return team != null ? MapTeamToProto(team) : new Protos.Team();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the team."));
        }
    }

    public override async Task<DeleteTeamResponse> DeleteTeam(DeleteTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var command = new DeleteTeamCommand { Id = id };
            var success = await _mediator.Send(command);
            return new DeleteTeamResponse
            {
                Success = success,
                Message = success ? "Team deleted." : "Team not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the team."));
        }
    }

    private Protos.Department MapDepartmentToProto(DepartmentDto dept)
    {
        return new Protos.Department
        {
            Id = dept.Id.ToString(),
            Name = dept.Name,
            CompanyId = dept.CompanyId.ToString(),
            ManagerId = dept.ManagerId?.ToString() ?? string.Empty,
            ManagerName = dept.ManagerName ?? string.Empty,
            CreatedAt = dept.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Description = dept.Description ?? string.Empty
        };
    }

    private Protos.Team MapTeamToProto(TeamDto team)
    {
        return new Protos.Team
        {
            Id = team.Id.ToString(),
            Name = team.Name,
            DepartmentId = team.DepartmentId.ToString(),
            ManagerId = team.LeaderId?.ToString() ?? string.Empty,
            ManagerName = team.LeaderName ?? string.Empty,
            CreatedAt = team.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Description = team.Description ?? string.Empty
        };
    }

    private EmployeeResponse MapToResponse(EmployeeDto employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id.ToString(),
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone ?? string.Empty,
            Position = employee.Position ?? string.Empty,
            Status = employee.Status ?? string.Empty,
            DepartmentId = employee.DepartmentId?.ToString() ?? string.Empty,
            TeamId = employee.TeamId?.ToString() ?? string.Empty,
            ManagerId = employee.ManagerId?.ToString() ?? string.Empty,
            HireDate = employee.HireDate?.ToString("yyyy-MM-dd") ?? string.Empty
        };
    }
}
