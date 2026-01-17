using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using EmployeeService.Infrastructure.Data;

namespace EmployeeService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EmployeeDbContext _context;
    private IEmployeeRepository? _employeeRepository;
    private IRepository<Company>? _companyRepository;
    private IRepository<Department>? _departmentRepository;
    private IRepository<Team>? _teamRepository;
    private IRepository<EmployeeRole>? _employeeRoleRepository;

    public UnitOfWork(EmployeeDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees =>
        _employeeRepository ??= new EmployeeRepository(_context);

    public IRepository<Company> Companies =>
        _companyRepository ??= new Repository<Company>(_context);

    public IRepository<Department> Departments =>
        _departmentRepository ??= new Repository<Department>(_context);

    public IRepository<Team> Teams =>
        _teamRepository ??= new Repository<Team>(_context);

    public IRepository<EmployeeRole> EmployeeRoles =>
        _employeeRoleRepository ??= new Repository<EmployeeRole>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
