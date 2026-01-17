using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using EmployeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(EmployeeDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByCodeAsync(string employeeCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _dbSet
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByManagerAsync(Guid managerId)
    {
        return await _dbSet
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetWithDepartmentAndTeamAsync()
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Team)
            .Include(e => e.Manager)
            .ToListAsync();
    }

    public async Task<Employee?> GetByKeycloakUserIdAsync(string keycloakUserId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.KeycloakUserId == keycloakUserId);
    }
}
