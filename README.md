# Employee Service

This service manages employee data, departments, teams, roles, and company holidays for the HRM system.

## Features

- CRUD for employees, departments, teams, companies
- Role assignment and RBAC integration (Keycloak)
- Organization chart data
- Audit log for changes
- Holiday management
- gRPC API for internal communication

## Tech Stack

- .NET 8, ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- gRPC
- Docker

## Endpoints

- REST: `/api/employees`, `/api/departments`, `/api/teams`, `/api/companies`, `/api/holidays`, `/api/auditlogs`
- gRPC: see `Protos/employee.proto`

## Database

- Connection: `Host=postgres-employee;Port=5432;Database=employee_db;Username=employee_user;Password=employee_pass`
- Seed data: `Data/seed-data.sql` (auto-applied on first run)

## Environment Variables

- `ConnectionStrings__DefaultConnection`
- `Keycloak__Authority`
- `Keycloak__Audience`

## Running Locally

```sh
docker-compose up -d postgres-employee
# (or run all infra)
dotnet ef database update --project EmployeeService
ASPNETCORE_ENVIRONMENT=Development dotnet run --project EmployeeService
```

## Docker

Service is built and run via Docker Compose. See root `docker-compose.yml`.

## Health Check

- `/health` endpoint for readiness/liveness

## Notes

- Requires Keycloak and PostgreSQL to be healthy before startup
- All changes are logged in `AuditLog` table
- Holidays are company-specific

---

© 2025 HRM System
