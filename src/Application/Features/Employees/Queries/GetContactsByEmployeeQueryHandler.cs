using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetContactsByEmployeeQueryHandler : IRequestHandler<GetContactsByEmployeeQuery, IEnumerable<ContactDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetContactsByEmployeeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetContactsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _unitOfWork.Contacts.GetAllAsync();
        return contacts
            .Where(c => c.EmployeeId == request.EmployeeId)
            .OrderByDescending(c => c.IsPrimary)
            .Select(c => new ContactDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                ContactName = c.ContactName,
                Relationship = c.Relationship,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                IsPrimary = c.IsPrimary
            });
    }
}
