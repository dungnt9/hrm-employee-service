using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _unitOfWork.Contacts.GetByIdAsync(request.ContactId);
        if (contact == null || contact.EmployeeId != request.EmployeeId)
            return false;

        // If setting this as primary, unset existing primary
        if (request.IsPrimary && !contact.IsPrimary)
        {
            var allContacts = await _unitOfWork.Contacts.GetAllAsync();
            var existingPrimary = allContacts
                .FirstOrDefault(c => c.EmployeeId == request.EmployeeId && c.IsPrimary && c.Id != request.ContactId);
            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
                await _unitOfWork.Contacts.UpdateAsync(existingPrimary);
            }
        }

        contact.ContactName = request.ContactName;
        contact.Relationship = request.Relationship;
        contact.Phone = request.Phone;
        contact.Email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
        contact.Address = string.IsNullOrEmpty(request.Address) ? null : request.Address;
        contact.IsPrimary = request.IsPrimary;

        await _unitOfWork.Contacts.UpdateAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
