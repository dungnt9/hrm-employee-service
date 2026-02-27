using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetDocumentsByEmployeeQueryHandler : IRequestHandler<GetDocumentsByEmployeeQuery, IEnumerable<DocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDocumentsByEmployeeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DocumentDto>> Handle(GetDocumentsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var docs = await _unitOfWork.Documents.GetAllAsync();
        return docs
            .Where(d => d.EmployeeId == request.EmployeeId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                DocumentType = d.DocumentType,
                DocumentName = d.DocumentName,
                FilePath = d.FilePath,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedBy = d.UploadedBy
            });
    }
}
