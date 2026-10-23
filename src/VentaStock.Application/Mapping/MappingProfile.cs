using AutoMapper;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserSessionDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Role.Permissions.Select(p => p.Name).ToList()));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<User, EditUserDto>()
            .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

        CreateMap<User, DeleteUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<EditCategoryDto, Category>();
        CreateMap<Category, EditCategoryDto>();
        CreateMap<Category, DeleteCategoryDto>();

        CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList()));
        CreateMap<Service, DeleteServiceDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList()));
        CreateMap<Service, EditServiceDto>()
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.Categories.Select(c => c.Id).ToList()));
    }
}
