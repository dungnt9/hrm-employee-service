using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDepartmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Departments.GetAllAsync();
        var dept = all.FirstOrDefault(d => d.Id == request.Id);
        if (dept == null) return null;

        return new DepartmentDto
        {
            Id = dept.Id,
            Name = dept.Name,
            Code = dept.Code,
            Description = dept.Description,
            CompanyId = dept.CompanyId,
            ManagerId = dept.ManagerId,
            ManagerName = dept.Manager != null ? $"{dept.Manager.FirstName} {dept.Manager.LastName}" : null,
            ParentDepartmentId = dept.ParentDepartmentId,
            CreatedAt = dept.CreatedAt,
            UpdatedAt = dept.UpdatedAt
        };
    }
}
