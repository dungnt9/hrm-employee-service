using AutoMapper;
using EmployeeService.Application.Features.Employees.DTOs;
using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.MappingProfiles;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender != null ? src.Gender.ToString() : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EmployeeType, opt => opt.MapFrom(src => src.EmployeeType.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
    }
}
