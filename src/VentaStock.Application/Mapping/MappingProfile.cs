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
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Role.Permissions.Select(p => p.Name).ToList()))
            .ForMember(dest => dest.BranchIds, opt => opt.MapFrom(src => src.Branches.Select(b => b.Id).ToList()));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.Branches, opt => opt.MapFrom(src => src.Branches.OrderBy(b => b.Name).Select(b => b.Name).ToList()));

        CreateMap<User, EditUserDto>()
            .ForMember(dest => dest.BranchIds, opt => opt.MapFrom(src => src.Branches.Select(b => b.Id).ToList()))
            .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

        CreateMap<User, DeleteUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
            .ForMember(dest => dest.Branches, opt => opt.MapFrom(src => src.Branches.OrderBy(b => b.Name).Select(b => b.Name).ToList()));

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
