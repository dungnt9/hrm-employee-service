using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Use first available company
        var companies = await _unitOfWork.Companies.GetAllAsync();
        var company = companies.FirstOrDefault();
        if (company == null)
            throw new InvalidOperationException("No company found in the system.");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Name.Replace(" ", "").ToUpper()[..Math.Min(10, request.Name.Replace(" ", "").Length)],
            Description = request.Description,
            CompanyId = company.Id,
            ManagerId = Guid.TryParse(request.ManagerId, out var managerId) ? managerId : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return department.Id;
    }
}
