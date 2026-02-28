namespace EmployeeService.Domain.Entities;

public class EmployeeSkill
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Technical, Soft Skills, Language, Certification
    public int ProficiencyLevel { get; set; } // 1-5 (Beginner to Expert)
    public DateTime? LastAssessedDate { get; set; }
    public string? AssessedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee? Employee { get; set; }
}
