using AutoMapper;
using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDepartmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = await _unitOfWork.Departments.GetAllAsync();

        var departments = query.ToList();

        // Filter by company if specified
        if (request.CompanyId.HasValue)
        {
            departments = departments.Where(d => d.CompanyId == request.CompanyId.Value).ToList();
        }

        var departmentDtos = new List<DepartmentDto>();

        foreach (var dept in departments)
        {
            var dto = new DepartmentDto
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
            departmentDtos.Add(dto);
        }

        return departmentDtos;
    }
}
