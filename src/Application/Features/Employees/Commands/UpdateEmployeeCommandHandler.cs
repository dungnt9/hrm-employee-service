using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Enums;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id);
        if (employee == null)
            return false;

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.DepartmentId = request.DepartmentId;
        employee.TeamId = request.TeamId;
        employee.Position = request.Position;
        employee.ManagerId = request.ManagerId;
        employee.Status = Enum.TryParse<EmployeeStatus>(request.Status, out var status)
            ? status
            : EmployeeStatus.Active;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
