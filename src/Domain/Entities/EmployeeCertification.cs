namespace EmployeeService.Domain.Entities;

public class EmployeeCertification
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public string? CertificationNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool RequiresRenewal { get; set; }
    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired, Revoked
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
