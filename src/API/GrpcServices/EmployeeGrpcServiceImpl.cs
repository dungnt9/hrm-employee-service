using AutoMapper;
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

    public EmployeeGrpcServiceImpl(IMediator mediator, IMapper mapper, ILogger<EmployeeGrpcServiceImpl> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
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
                    CreatedAt = dept.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
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
                    CreatedAt = team.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
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
