using AutoMapper;
using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.Commands;
using EmployeeService.Application.Features.Employees.DTOs;
using EmployeeService.Application.Features.Employees.Queries;
using EmployeeService.Domain.Entities;
using EmployeeService.Protos;
using Grpc.Core;
using MediatR;

namespace EmployeeService.GrpcServices;

public class EmployeeGrpcServiceImpl : EmployeeGrpc.EmployeeGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeGrpcServiceImpl> _logger;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeGrpcServiceImpl(IMediator mediator, IMapper mapper, ILogger<EmployeeGrpcServiceImpl> logger, IEmployeeRepository employeeRepository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
        _employeeRepository = employeeRepository;
    }

    public override async Task<EmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
            {
                _logger.LogWarning("Invalid employee ID format: {EmployeeId}", request.EmployeeId);
                return new EmployeeResponse();
            }

            var query = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(query);

            if (employee == null)
            {
                _logger.LogInformation("Employee not found: {EmployeeId}", employeeId);
                return new EmployeeResponse();
            }

            return MapToResponse(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the employee."));
        }
    }

    public override async Task<EmployeesResponse> GetEmployees(GetEmployeesRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllEmployeesQuery();
            var employees = await _mediator.Send(query);

            var employeeList = employees.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var deptId))
            {
                employeeList = employeeList.Where(e => e.DepartmentId == deptId).ToList();
            }

            if (!string.IsNullOrEmpty(request.TeamId) && Guid.TryParse(request.TeamId, out var teamId))
            {
                employeeList = employeeList.Where(e => e.TeamId == teamId).ToList();
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                employeeList = employeeList.Where(e =>
                    e.FirstName.ToLower().Contains(search) ||
                    e.LastName.ToLower().Contains(search) ||
                    e.Email.ToLower().Contains(search)).ToList();
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var statusFilter = request.Status.ToLower();
                employeeList = employeeList.Where(e =>
                    !string.IsNullOrEmpty(e.Status) && e.Status.ToLower() == statusFilter).ToList();
            }

            var totalCount = employeeList.Count;
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var paginatedEmployees = employeeList
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new EmployeesResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            response.Employees.AddRange(paginatedEmployees.Select(MapToResponse));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving employees."));
        }
    }

    public override async Task<EmployeeResponse> CreateEmployee(CreateEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateEmployeeCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                DepartmentId = string.IsNullOrEmpty(request.DepartmentId) ? null : Guid.Parse(request.DepartmentId),
                TeamId = string.IsNullOrEmpty(request.TeamId) ? null : Guid.Parse(request.TeamId),
                Position = request.Position,
                HireDate = string.IsNullOrEmpty(request.HireDate) ? null : DateTime.Parse(request.HireDate)
            };

            var employeeId = await _mediator.Send(command);
            var getQuery = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(getQuery);

            if (employee == null)
                return new EmployeeResponse();

            return MapToResponse(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the employee."));
        }
    }

    public override async Task<EmployeeResponse> UpdateEmployee(UpdateEmployeeRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
            {
                _logger.LogWarning("Invalid employee ID format: {EmployeeId}", request.EmployeeId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID format."));
            }

            var command = new UpdateEmployeeCommand
            {
                Id = employeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone,
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : null,
                TeamId = Guid.TryParse(request.TeamId, out var teamId) ? teamId : null,
                Position = string.IsNullOrEmpty(request.Position) ? null : request.Position,
                ManagerId = Guid.TryParse(request.ManagerId, out var managerId) ? managerId : null,
                Status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status,
            };

            var success = await _mediator.Send(command);
            if (!success)
            {
                _logger.LogInformation("Employee not found for update: {EmployeeId}", employeeId);
                return new EmployeeResponse();
            }

            var getQuery = new GetEmployeeByIdQuery(employeeId);
            var employee = await _mediator.Send(getQuery);
            if (employee == null)
                return new EmployeeResponse();

            return MapToResponse(employee);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the employee."));
        }
    }

    public override async Task<DepartmentsResponse> GetDepartments(GetDepartmentsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllDepartmentsQuery();

            if (!string.IsNullOrEmpty(request.CompanyId) && Guid.TryParse(request.CompanyId, out var companyId))
            {
                query.CompanyId = companyId;
            }

            var departments = await _mediator.Send(query);

            var response = new DepartmentsResponse();

            foreach (var dept in departments)
            {
                response.Departments.Add(new Protos.Department
                {
                    Id = dept.Id.ToString(),
                    Name = dept.Name,
                    CompanyId = dept.CompanyId.ToString(),
                    ManagerId = dept.ManagerId?.ToString() ?? string.Empty,
                    ManagerName = dept.ManagerName ?? string.Empty,
                    CreatedAt = dept.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = dept.Description ?? string.Empty
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving departments."));
        }
    }

    public override async Task<TeamsResponse> GetTeams(GetTeamsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAllTeamsQuery();

            if (!string.IsNullOrEmpty(request.DepartmentId) && Guid.TryParse(request.DepartmentId, out var departmentId))
            {
                query.DepartmentId = departmentId;
            }

            var teams = await _mediator.Send(query);

            var response = new TeamsResponse();

            foreach (var team in teams)
            {
                response.Teams.Add(new Protos.Team
                {
                    Id = team.Id.ToString(),
                    Name = team.Name,
                    DepartmentId = team.DepartmentId.ToString(),
                    ManagerId = team.LeaderId?.ToString() ?? string.Empty,
                    ManagerName = team.LeaderName ?? string.Empty,
                    CreatedAt = team.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = team.Description ?? string.Empty
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving teams."));
        }
    }

    public override async Task<EmployeeResponse> GetEmployeeByKeycloakId(GetEmployeeByKeycloakIdRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.KeycloakUserId))
                return new EmployeeResponse();

            var employee = await _employeeRepository.GetByKeycloakUserIdAsync(request.KeycloakUserId);
            if (employee == null)
                return new EmployeeResponse();

            var dto = _mapper.Map<EmployeeDto>(employee);
            return MapToResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by Keycloak ID {KeycloakUserId}", request.KeycloakUserId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the employee."));
        }
    }

    public override async Task<Protos.Department> GetDepartment(GetDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            if (dept == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Department not found."));

            return MapDepartmentToProto(dept);
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the department."));
        }
    }

    public override async Task<Protos.Department> CreateDepartment(CreateDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateDepartmentCommand
            {
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                ManagerId = string.IsNullOrEmpty(request.ManagerId) ? null : request.ManagerId
            };
            var id = await _mediator.Send(command);
            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            return dept != null ? MapDepartmentToProto(dept) : new Protos.Department();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the department."));
        }
    }

    public override async Task<Protos.Department> UpdateDepartment(UpdateDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var command = new UpdateDepartmentCommand
            {
                Id = id,
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                ManagerId = string.IsNullOrEmpty(request.ManagerId) ? null : request.ManagerId
            };
            var success = await _mediator.Send(command);
            if (!success)
                throw new RpcException(new Status(StatusCode.NotFound, "Department not found."));

            var dept = await _mediator.Send(new GetDepartmentByIdQuery(id));
            return dept != null ? MapDepartmentToProto(dept) : new Protos.Department();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the department."));
        }
    }

    public override async Task<DeleteDepartmentResponse> DeleteDepartment(DeleteDepartmentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DepartmentId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid department ID."));

            var command = new DeleteDepartmentCommand { Id = id };
            var success = await _mediator.Send(command);
            return new DeleteDepartmentResponse
            {
                Success = success,
                Message = success ? "Department deleted." : "Department not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", request.DepartmentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the department."));
        }
    }

    public override async Task<Protos.Team> GetTeam(GetTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            if (team == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found."));

            return MapTeamToProto(team);
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the team."));
        }
    }

    public override async Task<Protos.Team> CreateTeam(CreateTeamRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateTeamCommand
            {
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : Guid.Empty,
                LeaderId = string.IsNullOrEmpty(request.LeaderId) ? null : request.LeaderId
            };
            var id = await _mediator.Send(command);
            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            return team != null ? MapTeamToProto(team) : new Protos.Team();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the team."));
        }
    }

    public override async Task<Protos.Team> UpdateTeam(UpdateTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var command = new UpdateTeamCommand
            {
                Id = id,
                Name = request.Name,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                DepartmentId = string.IsNullOrEmpty(request.DepartmentId) ? null : request.DepartmentId,
                LeaderId = string.IsNullOrEmpty(request.LeaderId) ? null : request.LeaderId
            };
            var success = await _mediator.Send(command);
            if (!success)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found."));

            var team = await _mediator.Send(new GetTeamByIdQuery(id));
            return team != null ? MapTeamToProto(team) : new Protos.Team();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the team."));
        }
    }

    public override async Task<DeleteTeamResponse> DeleteTeam(DeleteTeamRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.TeamId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team ID."));

            var command = new DeleteTeamCommand { Id = id };
            var success = await _mediator.Send(command);
            return new DeleteTeamResponse
            {
                Success = success,
                Message = success ? "Team deleted." : "Team not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", request.TeamId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the team."));
        }
    }

    // ===== Documents =====

    public override async Task<DocumentsResponse> GetDocuments(GetDocumentsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID."));

            var docs = await _mediator.Send(new GetDocumentsByEmployeeQuery(employeeId));
            var response = new DocumentsResponse();
            response.Documents.AddRange(docs.Select(MapDocumentToProto));
            return response;
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving documents."));
        }
    }

    public override async Task<DocumentResponse> AddDocument(AddDocumentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID."));

            var command = new AddDocumentCommand
            {
                EmployeeId = employeeId,
                DocumentType = request.DocumentType,
                DocumentName = request.DocumentName,
                FilePath = request.FilePath,
                Description = string.IsNullOrEmpty(request.Description) ? null : request.Description,
                UploadedBy = Guid.TryParse(request.UploadedBy, out var uploaderId) ? uploaderId : null
            };

            var docId = await _mediator.Send(command);
            var docs = await _mediator.Send(new GetDocumentsByEmployeeQuery(employeeId));
            var doc = docs.FirstOrDefault(d => d.Id == docId);
            return doc != null ? MapDocumentToProto(doc) : new DocumentResponse();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document for employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding the document."));
        }
    }

    public override async Task<DeleteDocumentResponse> DeleteDocument(DeleteDocumentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DocumentId, out var documentId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid document ID."));

            var command = new DeleteDocumentCommand { DocumentId = documentId };
            var success = await _mediator.Send(command);
            return new DeleteDocumentResponse
            {
                Success = success,
                Message = success ? "Document deleted." : "Document not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", request.DocumentId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the document."));
        }
    }

    // ===== Emergency Contacts =====

    public override async Task<ContactsResponse> GetContacts(GetContactsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID."));

            var contacts = await _mediator.Send(new GetContactsByEmployeeQuery(employeeId));
            var response = new ContactsResponse();
            response.Contacts.AddRange(contacts.Select(MapContactToProto));
            return response;
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts for employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving contacts."));
        }
    }

    public override async Task<ContactResponse> AddContact(AddContactRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID."));

            var command = new AddContactCommand
            {
                EmployeeId = employeeId,
                ContactName = request.ContactName,
                Relationship = request.Relationship,
                Phone = request.Phone,
                Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
                Address = string.IsNullOrEmpty(request.Address) ? null : request.Address,
                IsPrimary = request.IsPrimary
            };

            var contactId = await _mediator.Send(command);
            var contacts = await _mediator.Send(new GetContactsByEmployeeQuery(employeeId));
            var contact = contacts.FirstOrDefault(c => c.Id == contactId);
            return contact != null ? MapContactToProto(contact) : new ContactResponse();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contact for employee {EmployeeId}", request.EmployeeId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding the contact."));
        }
    }

    public override async Task<ContactResponse> UpdateContact(UpdateContactRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ContactId, out var contactId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid contact ID."));

            if (!Guid.TryParse(request.EmployeeId, out var employeeId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid employee ID."));

            var command = new UpdateContactCommand
            {
                ContactId = contactId,
                EmployeeId = employeeId,
                ContactName = request.ContactName,
                Relationship = request.Relationship,
                Phone = request.Phone,
                Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
                Address = string.IsNullOrEmpty(request.Address) ? null : request.Address,
                IsPrimary = request.IsPrimary
            };

            var success = await _mediator.Send(command);
            if (!success)
                return new ContactResponse();

            var contacts = await _mediator.Send(new GetContactsByEmployeeQuery(employeeId));
            var contact = contacts.FirstOrDefault(c => c.Id == contactId);
            return contact != null ? MapContactToProto(contact) : new ContactResponse();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", request.ContactId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the contact."));
        }
    }

    public override async Task<DeleteContactResponse> DeleteContact(DeleteContactRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ContactId, out var contactId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid contact ID."));

            var command = new DeleteContactCommand { ContactId = contactId };
            var success = await _mediator.Send(command);
            return new DeleteContactResponse
            {
                Success = success,
                Message = success ? "Contact deleted." : "Contact not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", request.ContactId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the contact."));
        }
    }

    // ===== Announcements =====

    public override async Task<AnnouncementsResponse> GetAnnouncements(GetAnnouncementsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetAnnouncementsQuery
            {
                Category = string.IsNullOrEmpty(request.Category) ? null : request.Category,
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : null,
                IncludeExpired = request.IncludeExpired,
                Page = request.Page > 0 ? request.Page : 1,
                PageSize = request.PageSize > 0 ? request.PageSize : 20
            };

            var announcements = await _mediator.Send(query);
            var list = announcements.ToList();

            var response = new AnnouncementsResponse { TotalCount = list.Count };
            response.Announcements.AddRange(list.Select(MapAnnouncementToProto));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcements");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving announcements."));
        }
    }

    public override async Task<AnnouncementResponse> GetAnnouncement(GetAnnouncementRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid announcement ID."));

            var announcement = await _mediator.Send(new GetAnnouncementByIdQuery(id));
            if (announcement == null)
                return new AnnouncementResponse();

            return MapAnnouncementToProto(announcement);
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcement {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the announcement."));
        }
    }

    public override async Task<AnnouncementResponse> CreateAnnouncement(CreateAnnouncementRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.CreatedBy, out var createdBy))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid creator ID."));

            var command = new CreateAnnouncementCommand
            {
                Title = request.Title,
                Content = request.Content,
                Category = string.IsNullOrEmpty(request.Category) ? "General" : request.Category,
                IsPinned = request.IsPinned,
                ExpiresAt = string.IsNullOrEmpty(request.ExpiresAt) ? null : DateTime.Parse(request.ExpiresAt),
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : null,
                CreatedBy = createdBy
            };

            var announcementId = await _mediator.Send(command);
            var announcement = await _mediator.Send(new GetAnnouncementByIdQuery(announcementId));
            return announcement != null ? MapAnnouncementToProto(announcement) : new AnnouncementResponse();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the announcement."));
        }
    }

    public override async Task<AnnouncementResponse> UpdateAnnouncement(UpdateAnnouncementRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid announcement ID."));

            var command = new UpdateAnnouncementCommand
            {
                Id = id,
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                IsPinned = request.IsPinned,
                ExpiresAt = string.IsNullOrEmpty(request.ExpiresAt) ? null : DateTime.Parse(request.ExpiresAt),
                DepartmentId = Guid.TryParse(request.DepartmentId, out var deptId) ? deptId : null
            };

            var success = await _mediator.Send(command);
            if (!success)
                return new AnnouncementResponse();

            var announcement = await _mediator.Send(new GetAnnouncementByIdQuery(id));
            return announcement != null ? MapAnnouncementToProto(announcement) : new AnnouncementResponse();
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the announcement."));
        }
    }

    public override async Task<DeleteAnnouncementResponse> DeleteAnnouncement(DeleteAnnouncementRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid announcement ID."));

            var command = new DeleteAnnouncementCommand { Id = id };
            var success = await _mediator.Send(command);
            return new DeleteAnnouncementResponse
            {
                Success = success,
                Message = success ? "Announcement deleted." : "Announcement not found."
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting announcement {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the announcement."));
        }
    }

    // ===== Mapping helpers =====

    private Protos.Department MapDepartmentToProto(DepartmentDto dept)
    {
        return new Protos.Department
        {
            Id = dept.Id.ToString(),
            Name = dept.Name,
            CompanyId = dept.CompanyId.ToString(),
            ManagerId = dept.ManagerId?.ToString() ?? string.Empty,
            ManagerName = dept.ManagerName ?? string.Empty,
            CreatedAt = dept.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Description = dept.Description ?? string.Empty
        };
    }

    private Protos.Team MapTeamToProto(TeamDto team)
    {
        return new Protos.Team
        {
            Id = team.Id.ToString(),
            Name = team.Name,
            DepartmentId = team.DepartmentId.ToString(),
            ManagerId = team.LeaderId?.ToString() ?? string.Empty,
            ManagerName = team.LeaderName ?? string.Empty,
            CreatedAt = team.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Description = team.Description ?? string.Empty
        };
    }

    private DocumentResponse MapDocumentToProto(DocumentDto doc)
    {
        return new DocumentResponse
        {
            Id = doc.Id.ToString(),
            EmployeeId = doc.EmployeeId.ToString(),
            DocumentType = doc.DocumentType,
            DocumentName = doc.DocumentName,
            FilePath = doc.FilePath,
            Description = doc.Description ?? string.Empty,
            UploadedAt = doc.UploadedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UploadedBy = doc.UploadedBy?.ToString() ?? string.Empty
        };
    }

    private ContactResponse MapContactToProto(ContactDto contact)
    {
        return new ContactResponse
        {
            Id = contact.Id.ToString(),
            EmployeeId = contact.EmployeeId.ToString(),
            ContactName = contact.ContactName,
            Relationship = contact.Relationship,
            Phone = contact.Phone,
            Email = contact.Email ?? string.Empty,
            Address = contact.Address ?? string.Empty,
            IsPrimary = contact.IsPrimary
        };
    }

    private AnnouncementResponse MapAnnouncementToProto(AnnouncementDto a)
    {
        return new AnnouncementResponse
        {
            Id = a.Id.ToString(),
            Title = a.Title,
            Content = a.Content,
            Category = a.Category,
            IsPinned = a.IsPinned,
            ExpiresAt = a.ExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? string.Empty,
            DepartmentId = a.DepartmentId?.ToString() ?? string.Empty,
            DepartmentName = a.DepartmentName ?? string.Empty,
            CreatedBy = a.CreatedBy.ToString(),
            CreatedByName = a.CreatedByName ?? string.Empty,
            CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = a.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    private EmployeeResponse MapToResponse(EmployeeDto employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id.ToString(),
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone ?? string.Empty,
            Position = employee.Position ?? string.Empty,
            Status = employee.Status ?? string.Empty,
            DepartmentId = employee.DepartmentId?.ToString() ?? string.Empty,
            TeamId = employee.TeamId?.ToString() ?? string.Empty,
            ManagerId = employee.ManagerId?.ToString() ?? string.Empty,
            HireDate = employee.HireDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            CreatedAt = employee.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = employee.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            BaseSalary = employee.BaseSalary?.ToString("F2") ?? string.Empty,
            EmployeeCode = employee.EmployeeCode ?? string.Empty
        };
    }
}
