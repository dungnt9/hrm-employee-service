using AutoMapper;
using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.Features.Employees.DTOs;
using MediatR;

namespace EmployeeService.Application.Features.Employees.Queries;

public class GetEmployeesByManagerQueryHandler : IRequestHandler<GetEmployeesByManagerQuery, IEnumerable<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public GetEmployeesByManagerQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmployeeDto>> Handle(GetEmployeesByManagerQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetByManagerAsync(request.ManagerId);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }
}
