using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(request.Id);
        if (department == null)
            return false;

        department.Name = request.Name;
        department.Description = request.Description;
        department.ManagerId = Guid.TryParse(request.ManagerId, out var managerId) ? managerId : department.ManagerId;
        department.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Departments.UpdateAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
