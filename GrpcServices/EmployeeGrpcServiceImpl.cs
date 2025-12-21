using EmployeeService.Data;
using EmployeeService.Domain.Entities;
using EmployeeService.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.GrpcServices;

public class EmployeeGrpcServiceImpl : EmployeeGrpc.EmployeeGrpcBase
{
    private readonly EmployeeDbContext _context;
    private readonly ILogger<EmployeeGrpcServiceImpl> _logger;

    public EmployeeGrpcServiceImpl(EmployeeDbContext context, ILogger<EmployeeGrpcServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<EmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new EmployeeResponse();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Team)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
        {
            return new EmployeeResponse();
        }

        return MapToResponse(employee);
    }

    public override async Task<EmployeesResponse> GetEmployees(GetEmployeesRequest request, ServerCallContext context)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Team)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
        {
            query = query.Where(e => e.DepartmentId == deptId);
        }

        if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
        {
            query = query.Where(e => e.TeamId == teamId);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e => 
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new EmployeesResponse
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
        response.Employees.AddRange(employees.Select(MapToResponse));

        return response;
    }

    public override async Task<EmployeeResponse> CreateEmployee(CreateEmployeeRequest request, ServerCallContext context)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Position = request.Position,
            KeycloakUserId = request.KeycloakUserId,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
        {
            employee.DepartmentId = deptId;
        }

        if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
        {
            employee.TeamId = teamId;
        }

        if (!string.IsNullOrEmpty(request.ManagerId) && Guid.TryParse(request.ManagerId, out var managerId))
        {
            employee.ManagerId = managerId;
        }

        if (!string.IsNullOrEmpty(request.HireDate) && DateTime.TryParse(request.HireDate, out var hireDate))
        {
            employee.HireDate = hireDate;
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return await GetEmployee(new GetEmployeeRequest { EmployeeId = employee.Id.ToString() }, context);
    }

    public override async Task<EmployeeResponse> UpdateEmployee(UpdateEmployeeRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new EmployeeResponse();
        }

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            return new EmployeeResponse();
        }

        if (!string.IsNullOrEmpty(request.FirstName)) employee.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) employee.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Email)) employee.Email = request.Email;
        if (!string.IsNullOrEmpty(request.Phone)) employee.Phone = request.Phone;
        if (!string.IsNullOrEmpty(request.Position)) employee.Position = request.Position;

        if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
        {
            employee.DepartmentId = deptId;
        }

        if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
        {
            employee.TeamId = teamId;
        }

        if (!string.IsNullOrEmpty(request.ManagerId) && Guid.TryParse(request.ManagerId, out var managerId))
        {
            employee.ManagerId = managerId;
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
        {
            employee.Status = status;
        }

        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetEmployee(new GetEmployeeRequest { EmployeeId = employee.Id.ToString() }, context);
    }

    public override async Task<DeleteEmployeeResponse> DeleteEmployee(DeleteEmployeeRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new DeleteEmployeeResponse { Success = false, Message = "Invalid employee ID" };
        }

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            return new DeleteEmployeeResponse { Success = false, Message = "Employee not found" };
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return new DeleteEmployeeResponse { Success = true, Message = "Employee deleted successfully" };
    }

    public override async Task<OrgChartResponse> GetOrgChart(GetOrgChartRequest request, ServerCallContext context)
    {
        var companies = await _context.Companies
            .Include(c => c.Departments)
                .ThenInclude(d => d.Teams)
                    .ThenInclude(t => t.Employees)
                        .ThenInclude(e => e.Manager)
            .Include(c => c.Departments)
                .ThenInclude(d => d.Manager)
            .ToListAsync();

        var company = companies.FirstOrDefault();
        if (company == null)
        {
            return new OrgChartResponse();
        }

        var root = new OrgChartNode
        {
            Id = company.Id.ToString(),
            Name = company.Name,
            Type = "company",
            ParentId = ""
        };

        foreach (var dept in company.Departments)
        {
            var deptNode = new OrgChartNode
            {
                Id = dept.Id.ToString(),
                Name = dept.Name,
                Type = "department",
                ParentId = company.Id.ToString()
            };

            foreach (var team in dept.Teams)
            {
                var teamNode = new OrgChartNode
                {
                    Id = team.Id.ToString(),
                    Name = team.Name,
                    Type = "team",
                    ParentId = dept.Id.ToString()
                };

                foreach (var emp in team.Employees)
                {
                    var empNode = new OrgChartNode
                    {
                        Id = emp.Id.ToString(),
                        Name = $"{emp.FirstName} {emp.LastName}",
                        Type = "employee",
                        ParentId = team.Id.ToString(),
                        EmployeeData = MapToResponse(emp)
                    };
                    teamNode.Children.Add(empNode);
                }

                deptNode.Children.Add(teamNode);
            }

            root.Children.Add(deptNode);
        }

        return new OrgChartResponse { Root = root };
    }

    public override async Task<EmployeesResponse> GetTeamMembers(GetTeamMembersRequest request, ServerCallContext context)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Team)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
        {
            query = query.Where(e => e.TeamId == teamId);
        }
        else if (!string.IsNullOrEmpty(request.ManagerId) && Guid.TryParse(request.ManagerId, out var managerId))
        {
            query = query.Where(e => e.ManagerId == managerId);
        }

        var employees = await query.ToListAsync();

        var response = new EmployeesResponse
        {
            TotalCount = employees.Count,
            Page = 1,
            PageSize = employees.Count
        };
        response.Employees.AddRange(employees.Select(MapToResponse));

        return response;
    }

    public override async Task<EmployeeResponse> GetEmployeeManager(GetEmployeeManagerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new EmployeeResponse();
        }

        var employee = await _context.Employees
            .Include(e => e.Manager)
                .ThenInclude(m => m!.Department)
            .Include(e => e.Manager)
                .ThenInclude(m => m!.Team)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee?.Manager == null)
        {
            return new EmployeeResponse();
        }

        return MapToResponse(employee.Manager);
    }

    public override async Task<ValidateManagerPermissionResponse> ValidateManagerPermission(ValidateManagerPermissionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ManagerId, out var managerId) || !Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new ValidateManagerPermissionResponse { IsValid = false, Message = "Invalid IDs" };
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.ManagerId == managerId);

        if (employee != null)
        {
            return new ValidateManagerPermissionResponse { IsValid = true, Message = "Manager has permission" };
        }

        // Check if manager is team lead of employee's team
        var manager = await _context.Teams
            .AnyAsync(t => t.LeaderId == managerId &&
                          _context.Employees.Any(e => e.Id == employeeId && e.TeamId == t.Id));

        if (manager)
        {
            return new ValidateManagerPermissionResponse { IsValid = true, Message = "Manager has team permission" };
        }

        return new ValidateManagerPermissionResponse { IsValid = false, Message = "Manager does not have permission" };
    }

    public override async Task<AssignRoleResponse> AssignRole(AssignRoleRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeId))
        {
            return new AssignRoleResponse { Success = false, Message = "Invalid employee ID" };
        }

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            return new AssignRoleResponse { Success = false, Message = "Employee not found" };
        }

        var existingRole = await _context.EmployeeRoles
            .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.Role == request.Role);

        if (existingRole != null)
        {
            return new AssignRoleResponse { Success = true, Message = "Role already assigned" };
        }

        _context.EmployeeRoles.Add(new EmployeeRole
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Role = request.Role,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new AssignRoleResponse { Success = true, Message = "Role assigned successfully" };
    }

    public override async Task<DepartmentsResponse> GetDepartments(GetDepartmentsRequest request, ServerCallContext context)
    {
        var query = _context.Departments
            .Include(d => d.Manager)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.CompanyId) && Guid.TryParse(request.CompanyId, out var companyId))
        {
            query = query.Where(d => d.CompanyId == companyId);
        }

        var departments = await query.ToListAsync();

        var response = new DepartmentsResponse();
        response.Departments.AddRange(departments.Select(d => new Protos.Department
        {
            Id = d.Id.ToString(),
            Name = d.Name,
            CompanyId = d.CompanyId.ToString(),
            ManagerId = d.ManagerId?.ToString() ?? "",
            ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : "",
            CreatedAt = d.CreatedAt.ToString("O")
        }));

        return response;
    }

    public override async Task<TeamsResponse> GetTeams(GetTeamsRequest request, ServerCallContext context)
    {
        var query = _context.Teams
            .Include(t => t.Leader)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
        {
            query = query.Where(t => t.DepartmentId == deptId);
        }

        var teams = await query.ToListAsync();

        var response = new TeamsResponse();
        response.Teams.AddRange(teams.Select(t => new Protos.Team
        {
            Id = t.Id.ToString(),
            Name = t.Name,
            DepartmentId = t.DepartmentId.ToString(),
            ManagerId = t.LeaderId?.ToString() ?? "",
            ManagerName = t.Leader != null ? $"{t.Leader.FirstName} {t.Leader.LastName}" : "",
            CreatedAt = t.CreatedAt.ToString("O")
        }));

        return response;
    }

    private static EmployeeResponse MapToResponse(Employee e) => new()
    {
        Id = e.Id.ToString(),
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone ?? "",
        DepartmentId = e.DepartmentId?.ToString() ?? "",
        DepartmentName = e.Department?.Name ?? "",
        TeamId = e.TeamId?.ToString() ?? "",
        TeamName = e.Team?.Name ?? "",
        Position = e.Position ?? "",
        ManagerId = e.ManagerId?.ToString() ?? "",
        ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : "",
        KeycloakUserId = e.KeycloakUserId ?? "",
        HireDate = e.HireDate?.ToString("yyyy-MM-dd") ?? "",
        Status = e.Status.ToString(),
        CreatedAt = e.CreatedAt.ToString("O"),
        UpdatedAt = e.UpdatedAt.ToString("O")
    };
}
