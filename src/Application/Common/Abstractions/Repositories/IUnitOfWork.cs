using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Common.Abstractions.Repositories;

public interface IUnitOfWork
{
    IEmployeeRepository Employees { get; }
    IRepository<Company> Companies { get; }
    IRepository<Department> Departments { get; }
    IRepository<Team> Teams { get; }
    IRepository<EmployeeRole> EmployeeRoles { get; }
    IRepository<EmployeeDocument> Documents { get; }
    IRepository<EmployeeContact> Contacts { get; }
    IRepository<Announcement> Announcements { get; }

    Task<int> SaveChangesAsync();
}
