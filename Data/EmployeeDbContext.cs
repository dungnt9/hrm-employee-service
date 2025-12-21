using EmployeeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Data;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(e => e.Departments)
                .WithOne(d => d.Company)
                .HasForeignKey(d => d.CompanyId);
        });

        // Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.Teams)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId);
        });

        // Team
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Leader)
                .WithMany()
                .HasForeignKey(e => e.LeaderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.KeycloakUserId);
            
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Team)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Manager)
                .WithMany(m => m.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EmployeeRole
        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.EmployeeRoles)
                .HasForeignKey(e => e.EmployeeId);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.PerformedByEmployee)
                .WithMany()
                .HasForeignKey(e => e.PerformedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Holiday
        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => new { e.CompanyId, e.Date });
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var dept1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dept2Id = Guid.Parse("22222222-2222-2222-2222-222222222223");
        var team1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var team2Id = Guid.Parse("33333333-3333-3333-3333-333333333334");
        var adminId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var hrId = Guid.Parse("44444444-4444-4444-4444-444444444445");
        var managerId = Guid.Parse("44444444-4444-4444-4444-444444444446");
        var empId = Guid.Parse("44444444-4444-4444-4444-444444444447");

        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = companyId,
            Name = "HRM Company",
            Description = "Main company",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Id = dept1Id,
                Name = "Engineering",
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = dept2Id,
                Name = "Human Resources",
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Team>().HasData(
            new Team
            {
                Id = team1Id,
                Name = "Backend Team",
                DepartmentId = dept1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Team
            {
                Id = team2Id,
                Name = "Frontend Team",
                DepartmentId = dept1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = adminId,
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@hrm.local",
                KeycloakUserId = "admin",
                Position = "System Administrator",
                Status = EmployeeStatus.Active,
                HireDate = DateTime.UtcNow.AddYears(-5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = hrId,
                FirstName = "HR",
                LastName = "Staff",
                Email = "hr@hrm.local",
                KeycloakUserId = "hr_user",
                DepartmentId = dept2Id,
                Position = "HR Manager",
                Status = EmployeeStatus.Active,
                HireDate = DateTime.UtcNow.AddYears(-3),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = managerId,
                FirstName = "Team",
                LastName = "Manager",
                Email = "manager@hrm.local",
                KeycloakUserId = "manager_user",
                DepartmentId = dept1Id,
                TeamId = team1Id,
                Position = "Team Lead",
                Status = EmployeeStatus.Active,
                HireDate = DateTime.UtcNow.AddYears(-2),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = empId,
                FirstName = "Normal",
                LastName = "Employee",
                Email = "employee@hrm.local",
                KeycloakUserId = "employee_user",
                DepartmentId = dept1Id,
                TeamId = team1Id,
                ManagerId = managerId,
                Position = "Software Developer",
                Status = EmployeeStatus.Active,
                HireDate = DateTime.UtcNow.AddYears(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<EmployeeRole>().HasData(
            new EmployeeRole { Id = Guid.NewGuid(), EmployeeId = adminId, Role = "system_admin", AssignedAt = DateTime.UtcNow },
            new EmployeeRole { Id = Guid.NewGuid(), EmployeeId = hrId, Role = "hr_staff", AssignedAt = DateTime.UtcNow },
            new EmployeeRole { Id = Guid.NewGuid(), EmployeeId = managerId, Role = "manager", AssignedAt = DateTime.UtcNow },
            new EmployeeRole { Id = Guid.NewGuid(), EmployeeId = empId, Role = "employee", AssignedAt = DateTime.UtcNow }
        );
    }
}
