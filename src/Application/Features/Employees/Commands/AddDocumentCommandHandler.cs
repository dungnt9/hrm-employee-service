using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Domain.Entities;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class AddDocumentCommandHandler : IRequestHandler<AddDocumentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = new EmployeeDocument
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            DocumentType = request.DocumentType,
            DocumentName = request.DocumentName,
            FilePath = request.FilePath,
            Description = request.Description,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = request.UploadedBy
        };

        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();
        return document.Id;
    }
}
