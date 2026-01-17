using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Common.Abstractions.Repositories;

public interface IUnitOfWork
{
    IEmployeeRepository Employees { get; }
    IRepository<Company> Companies { get; }
    IRepository<Department> Departments { get; }
    IRepository<Team> Teams { get; }
    IRepository<EmployeeRole> EmployeeRoles { get; }

    Task<int> SaveChangesAsync();
}
