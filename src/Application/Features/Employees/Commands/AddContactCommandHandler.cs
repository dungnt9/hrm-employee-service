using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Application.Features.Employees.Commands;

public class AddContactCommandHandler : IRequestHandler<AddContactCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        // If new contact is primary, unset existing primary
        if (request.IsPrimary)
        {
            var existingContacts = await _unitOfWork.Contacts.GetAllAsync();
            var existingPrimary = existingContacts
                .FirstOrDefault(c => c.EmployeeId == request.EmployeeId && c.IsPrimary);
            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
                await _unitOfWork.Contacts.UpdateAsync(existingPrimary);
            }
        }

        var contact = new EmployeeContact
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ContactName = request.ContactName,
            Relationship = request.Relationship,
            Phone = request.Phone,
            Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
            Address = string.IsNullOrEmpty(request.Address) ? null : request.Address,
            IsPrimary = request.IsPrimary
        };

        await _unitOfWork.Contacts.AddAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        return contact.Id;
    }
}
