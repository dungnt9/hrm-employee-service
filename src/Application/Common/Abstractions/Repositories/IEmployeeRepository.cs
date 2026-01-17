using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Common.Abstractions.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByCodeAsync(string employeeCode);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId);
    Task<IEnumerable<Employee>> GetByManagerAsync(Guid managerId);
    Task<IEnumerable<Employee>> GetWithDepartmentAndTeamAsync();
    Task<Employee?> GetByKeycloakUserIdAsync(string keycloakUserId);
}
