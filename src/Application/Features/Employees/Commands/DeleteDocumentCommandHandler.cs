using EmployeeService.Application.Common.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Application.Features.Employees.Commands;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId);
        if (document == null || document.EmployeeId != request.EmployeeId)
            return false;

        await _unitOfWork.Documents.DeleteAsync(document);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
