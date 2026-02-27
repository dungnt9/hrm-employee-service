using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _unitOfWork.Contacts.GetByIdAsync(request.ContactId);
        if (contact == null || contact.EmployeeId != request.EmployeeId)
            return false;

        await _unitOfWork.Contacts.DeleteAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
