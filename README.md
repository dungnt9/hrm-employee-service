# Employee Service

gRPC microservice quản lý thông tin nhân viên, phòng ban, team và công ty cho hệ thống HRM.

## Mục lục

- [Kiến trúc](#kiến-trúc)
- [Công nghệ](#công-nghệ)
- [Nghiệp vụ](#nghiệp-vụ)
- [gRPC API](#grpc-api)
- [CQRS Pattern](#cqrs-pattern)
- [Domain Entities](#domain-entities)
- [Database Schema](#database-schema)
- [Luồng xử lý](#luồng-xử-lý)
- [Cấu hình](#cấu-hình)
- [Chạy ứng dụng](#chạy-ứng-dụng)

---

## Kiến trúc

**Clean Architecture 4-Layer Pattern:**

```
src/
├── API/                        # Layer 1: Presentation
│   ├── GrpcServices/           # gRPC service implementations
│   ├── Protos/                 # Protocol buffer definitions
│   └── Program.cs              # Entry point & DI configuration
│
├── Application/                # Layer 2: Business Logic
│   ├── Features/               # CQRS Commands & Queries
│   │   └── Employees/
│   │       ├── Commands/       # CreateEmployee, UpdateEmployee, DeleteEmployee
│   │       ├── Queries/        # GetEmployeeById, GetAllEmployees, GetByDepartment...
│   │       └── DTOs/           # Data Transfer Objects
│   ├── Common/
│   │   └── Abstractions/       # Repository interfaces
│   └── MappingProfiles/        # AutoMapper profiles
│
├── Domain/                     # Layer 3: Enterprise Business Rules
│   ├── Entities/               # Employee, Department, Team, Company...
│   └── Enums/                  # EmployeeStatus, EmployeeType, Gender
│
└── Infrastructure/             # Layer 4: Data Access
    ├── Data/                   # DbContext, Configurations
    └── Repositories/           # Repository implementations
```

---

## Công nghệ

| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|----------|
| .NET | 8.0 | Framework |
| ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0 | ORM |
| PostgreSQL | 16 | Database |
| gRPC | - | Inter-service communication |
| MediatR | 12.x | CQRS pattern implementation |
| AutoMapper | 13.x | Object mapping |
| FluentValidation | 11.x | Input validation |
| Serilog | - | Structured logging |
| Keycloak | 23.0 | JWT Authentication & SSO |

---

## Nghiệp vụ

### Quản lý nhân viên

| Chức năng | Mô tả | Quyền yêu cầu |
|-----------|-------|---------------|
| Tạo nhân viên | Thêm nhân viên mới vào hệ thống | `hr_staff` |
| Cập nhật thông tin | Sửa thông tin cá nhân, vị trí, phòng ban | `hr_staff` |
| Xóa nhân viên | Xóa nhân viên khỏi hệ thống | `system_admin` |
| Xem thông tin | Xem chi tiết nhân viên | `employee` (bản thân), `manager` (team), `hr_staff` (tất cả) |
| Gán vai trò | Phân quyền Keycloak cho nhân viên | `system_admin` |

### Quản lý tổ chức

| Chức năng | Mô tả |
|-----------|-------|
| Phòng ban | Quản lý cấu trúc phòng ban (hỗ trợ phòng ban con) |
| Team | Quản lý team trong phòng ban |
| Sơ đồ tổ chức | Hiển thị cấu trúc cây tổ chức |
| Quan hệ quản lý | Xác định cấp trên - cấp dưới |

### Trạng thái nhân viên

```
Active ←→ OnLeave ←→ Inactive
  ↓
Probation → Active
  ↓
Terminated / Resigned (final states)
```

### Loại hình làm việc

- **FullTime**: Nhân viên chính thức toàn thời gian
- **PartTime**: Nhân viên bán thời gian
- **Contract**: Nhân viên hợp đồng
- **Temporary**: Nhân viên tạm thời
- **Intern**: Thực tập sinh

---

## gRPC API

### Service: EmployeeGrpc

#### Quản lý nhân viên

| Method | Request | Response | Mô tả |
|--------|---------|----------|-------|
| `GetEmployee` | `GetEmployeeRequest` | `EmployeeResponse` | Lấy thông tin nhân viên theo ID |
| `GetEmployees` | `GetEmployeesRequest` | `EmployeesResponse` | Danh sách nhân viên (phân trang, lọc) |
| `CreateEmployee` | `CreateEmployeeRequest` | `EmployeeResponse` | Tạo nhân viên mới |
| `UpdateEmployee` | `UpdateEmployeeRequest` | `EmployeeResponse` | Cập nhật thông tin |
| `DeleteEmployee` | `DeleteEmployeeRequest` | `DeleteEmployeeResponse` | Xóa nhân viên |

#### Cấu trúc tổ chức

| Method | Request | Response | Mô tả |
|--------|---------|----------|-------|
| `GetOrgChart` | `GetOrgChartRequest` | `OrgChartResponse` | Lấy sơ đồ tổ chức |
| `GetTeamMembers` | `GetTeamMembersRequest` | `EmployeesResponse` | Danh sách thành viên team |
| `GetEmployeeManager` | `GetEmployeeManagerRequest` | `EmployeeResponse` | Lấy thông tin quản lý |
| `ValidateManagerPermission` | `ValidateManagerPermissionRequest` | `ValidateManagerPermissionResponse` | Kiểm tra quyền quản lý |

#### Quản trị

| Method | Request | Response | Mô tả |
|--------|---------|----------|-------|
| `AssignRole` | `AssignRoleRequest` | `AssignRoleResponse` | Gán vai trò cho nhân viên |
| `GetDepartments` | `GetDepartmentsRequest` | `DepartmentsResponse` | Danh sách phòng ban |
| `GetTeams` | `GetTeamsRequest` | `TeamsResponse` | Danh sách team |

### Request Examples

```protobuf
// Lấy danh sách nhân viên với filter
message GetEmployeesRequest {
  int32 page = 1;           // Trang (mặc định: 1)
  int32 page_size = 2;      // Số item/trang (mặc định: 10)
  string department_id = 3; // Lọc theo phòng ban
  string team_id = 4;       // Lọc theo team
  string search = 5;        // Tìm theo tên, email
}

// Tạo nhân viên mới
message CreateEmployeeRequest {
  string first_name = 1;
  string last_name = 2;
  string email = 3;
  string phone = 4;
  string department_id = 5;
  string team_id = 6;
  string position = 7;
  string manager_id = 8;
  string keycloak_user_id = 9;
  string hire_date = 10;    // Format: yyyy-MM-dd
}
```

---

## CQRS Pattern

### Commands

| Command | Input | Output | Mô tả |
|---------|-------|--------|-------|
| `CreateEmployeeCommand` | FirstName, LastName, Email, Phone, DepartmentId, TeamId, Position, HireDate, EmployeeType | `Guid` | Tạo nhân viên mới |
| `UpdateEmployeeCommand` | Id, FirstName, LastName, Email, Phone, DepartmentId, TeamId, Position, ManagerId, Status | `bool` | Cập nhật thông tin |
| `DeleteEmployeeCommand` | Id | `bool` | Xóa nhân viên |

### Queries

| Query | Input | Output | Mô tả |
|-------|-------|--------|-------|
| `GetEmployeeByIdQuery` | Id | `EmployeeDto?` | Lấy nhân viên theo ID |
| `GetAllEmployeesQuery` | - | `IEnumerable<EmployeeDto>` | Lấy tất cả nhân viên |
| `GetEmployeesByDepartmentQuery` | DepartmentId | `IEnumerable<EmployeeDto>` | Lấy nhân viên theo phòng ban |
| `GetEmployeesByManagerQuery` | ManagerId | `IEnumerable<EmployeeDto>` | Lấy nhân viên cấp dưới |

### EmployeeDto

```csharp
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }  // Computed
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ManagerId { get; set; }
    public string Status { get; set; }      // Enum as string
    public string EmployeeType { get; set; } // Enum as string
    public string? Gender { get; set; }      // Enum as string
    public DateTime? HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## Domain Entities

### Employee (Core Entity)

```csharp
public class Employee
{
    // Identity
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; }

    // Personal Info
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }           // UNIQUE
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }

    // Organization
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ManagerId { get; set; }        // Self-reference

    // Employment
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public EmployeeType EmployeeType { get; set; }

    // Financial
    public decimal? BaseSalary { get; set; }
    public string? BankAccount { get; set; }
    public string? TaxCode { get; set; }
    public string? SocialInsuranceNumber { get; set; }

    // Integration
    public string? KeycloakUserId { get; set; }

    // Navigation
    public Department? Department { get; set; }
    public Team? Team { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; }
    public ICollection<EmployeeRole> EmployeeRoles { get; set; }
}
```

### Các Entity khác

| Entity | Mô tả | Quan hệ |
|--------|-------|---------|
| `Company` | Thông tin công ty | 1:N với Department |
| `Department` | Phòng ban | N:1 Company, 1:N Team, 1:N Employee, Self-reference (phòng ban con) |
| `Team` | Team/Nhóm | N:1 Department, 1:N Employee |
| `EmployeeRole` | Vai trò nhân viên | N:1 Employee |
| `EmployeeContact` | Liên hệ khẩn cấp | N:1 Employee |
| `EmployeeDocument` | Tài liệu nhân viên | N:1 Employee |
| `AuditLog` | Lịch sử thay đổi | Tracking tất cả entity |
| `Holiday` | Ngày nghỉ lễ | N:1 Company |

---

## Database Schema

### ERD Diagram

```
┌─────────────┐       ┌─────────────────┐       ┌──────────────┐
│  Companies  │───1:N─│   Departments   │───1:N─│    Teams     │
└─────────────┘       └─────────────────┘       └──────────────┘
                             │ 1:N                     │ 1:N
                             ▼                         ▼
                      ┌─────────────────────────────────────┐
                      │             Employees                │
                      │  (self-reference: Manager/Subordinates)
                      └─────────────────────────────────────┘
                        │ 1:N      │ 1:N        │ 1:N
                        ▼          ▼            ▼
              ┌──────────────┐ ┌──────────┐ ┌──────────────────┐
              │EmployeeRoles │ │ Contacts │ │EmployeeDocuments │
              └──────────────┘ └──────────┘ └──────────────────┘
```

### Bảng chính

| Bảng | Columns chính | Indexes |
|------|---------------|---------|
| `employees` | id, employee_code, first_name, last_name, email, department_id, team_id, manager_id, status, employee_type | UNIQUE(email), INDEX(keycloak_user_id) |
| `departments` | id, name, code, company_id, manager_id, parent_department_id, is_active | COMPOSITE(company_id, code) |
| `teams` | id, name, code, department_id, leader_id, is_active | |
| `companies` | id, name, code, is_active | |
| `employee_roles` | id, employee_id, role, assigned_at | |
| `audit_logs` | id, entity_type, entity_id, action, old_values, new_values, timestamp | INDEX(entity_id), INDEX(timestamp) |

### Connection String

```
Host=postgres-employee;Port=5432;Database=employee_db;Username=employee_user;Password=employee_pass
```

---

## Luồng xử lý

### Tạo nhân viên mới

```
┌─────────────┐     ┌─────────────────┐     ┌───────────────────┐
│ API Gateway │────>│ EmployeeGrpc    │────>│ CreateEmployeeCmd │
│   (REST)    │     │   Service       │     │     Handler       │
└─────────────┘     └─────────────────┘     └───────────────────┘
                                                     │
                    ┌───────────────────┐            │
                    │  EmployeeResponse │<───────────┤
                    │    (gRPC)         │            ▼
                    └───────────────────┘     ┌───────────────┐
                                              │  Repository   │
                                              │ (PostgreSQL)  │
                                              └───────────────┘
```

### Xác thực Manager Permission

```
┌─────────────┐     ┌──────────────────────────┐     ┌────────────────┐
│ Time Service│────>│ ValidateManagerPermission│────>│ Check if       │
│ (Approve    │     │                          │     │ employee.Manager│
│  Leave)     │     └──────────────────────────┘     │ == managerId   │
└─────────────┘                                      └────────────────┘
```

### Integration Flow

```
┌──────────────┐         ┌───────────────────┐
│   Frontend   │◄───────>│    API Gateway    │
│  (Next.js)   │  REST   │  (REST/GraphQL)   │
└──────────────┘         └───────────────────┘
                                  │
                    ┌─────────────┴─────────────┐
                    │           gRPC            │
                    ▼                           ▼
          ┌─────────────────┐         ┌─────────────────┐
          │Employee Service │         │  Time Service   │
          │  (PostgreSQL)   │         │  (PostgreSQL)   │
          └─────────────────┘         └─────────────────┘
                    │
                    ▼
          ┌─────────────────┐
          │    Keycloak     │
          │  (JWT Validate) │
          └─────────────────┘
```

---

## Cấu hình

### Environment Variables

| Variable | Mô tả | Giá trị mặc định |
|----------|-------|------------------|
| `ASPNETCORE_ENVIRONMENT` | Môi trường | Development |
| `ASPNETCORE_URLS` | URLs lắng nghe | http://+:8080;http://+:8081 |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | - |
| `Keycloak__Authority` | Keycloak realm URL | http://keycloak:8080/realms/hrm |
| `Keycloak__Audience` | API audience | hrm-api |
| `Keycloak__ClientId` | Client ID | hrm-api |
| `Keycloak__ClientSecret` | Client secret | hrm-api-secret |
| `Keycloak__RequireHttps` | Yêu cầu HTTPS | false |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=employee_db;Username=employee_user;Password=employee_pass"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/hrm",
    "Audience": "hrm-api",
    "ClientId": "hrm-api",
    "ClientSecret": "hrm-api-secret",
    "RequireHttps": false
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Grpc": "Information"
      }
    }
  }
}
```

### Keycloak Roles

| Role | Permissions |
|------|-------------|
| `employee` | Xem thông tin bản thân |
| `manager` | Xem thông tin team members |
| `hr_staff` | CRUD tất cả nhân viên |
| `system_admin` | Full access + gán vai trò |

---

## Chạy ứng dụng

### Với Docker Compose (Khuyến nghị)

```bash
# Từ thư mục hrm-deployment
cd hrm-deployment

# Chạy toàn bộ hệ thống
docker compose up -d

# Hoặc chỉ chạy Employee Service + dependencies
docker compose up -d postgres-employee keycloak employee-service
```

### Local Development

```bash
# 1. Start dependencies
cd hrm-deployment
docker compose up -d postgres-employee keycloak

# 2. Run migrations (nếu có)
cd ../hrm-employee-service
dotnet ef database update --project src/Infrastructure --startup-project src/API

# 3. Run service
dotnet run --project src/API
```

### Docker Build

```bash
# Build image
docker build -t hrm-employee-service .

# Run container
docker run -p 5001:8080 -p 5002:8081 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=employee_db;Username=employee_user;Password=employee_pass" \
  -e Keycloak__Authority="http://host.docker.internal:8080/realms/hrm" \
  hrm-employee-service
```

### Ports

| Port | Protocol | Mô tả |
|------|----------|-------|
| 8080 (external: 5001) | HTTP | Health check endpoint |
| 8081 (external: 5002) | gRPC | gRPC endpoints |

### Health Check

```bash
# HTTP health check
curl http://localhost:5001/health

# gRPC health check (with grpcurl)
grpcurl -plaintext localhost:5002 grpc.health.v1.Health/Check
```

---

## Seed Data

Hệ thống được seed với dữ liệu mẫu gồm:

- **7 Phòng ban**: Executive, Engineering, HR, Finance, Marketing, Operations, QA
- **14 Teams**: Backend, Frontend, Mobile, Data, Recruitment, HR Operations, Accounting, Payroll, Digital Marketing, Content, DevOps, IT Support, Manual QA, Automation QA
- **30 Nhân viên**: Với đầy đủ thông tin cá nhân, vị trí, quan hệ quản lý

### Tài khoản mẫu

| Vai trò | Email | Mật khẩu |
|---------|-------|----------|
| CEO | minh.nguyen@hrm.vn | (Keycloak) |
| HR Manager | lan.pham@hrm.vn | (Keycloak) |
| Engineering Manager | tuan.le@hrm.vn | (Keycloak) |
| System Admin | admin@hrm.vn | admin |

---

## Troubleshooting

### Lỗi kết nối Database

```bash
# Kiểm tra container PostgreSQL
docker logs hrm-postgres-employee

# Kiểm tra kết nối
docker exec -it hrm-postgres-employee psql -U employee_user -d employee_db -c "\dt"
```

### Lỗi gRPC

```bash
# Test gRPC với grpcurl
grpcurl -plaintext -d '{"employee_id": "guid-here"}' \
  localhost:5002 employee.EmployeeGrpc/GetEmployee
```

### Lỗi Keycloak

```bash
# Kiểm tra Keycloak health
curl http://localhost:8080/health/ready

# Kiểm tra realm
curl http://localhost:8080/realms/hrm/.well-known/openid-configuration
```

---

© 2025 HRM System - Clean Architecture
