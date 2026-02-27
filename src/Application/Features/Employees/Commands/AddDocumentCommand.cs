using MediatR;

namespace EmployeeService.Application.Features.Employees.Commands;

public class AddDocumentCommand : IRequest<Guid>
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UploadedBy { get; set; }
}
