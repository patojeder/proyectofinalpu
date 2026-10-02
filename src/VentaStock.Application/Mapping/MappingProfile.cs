using AutoMapper;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Sesión / Login
        CreateMap<Usuario, UsuarioSesionDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.IdRolNavigation.Nombre))
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src => src.IdRolNavigation.IdPermisos.Select(p => p.Nombre).ToList()));

        // Listado general
        CreateMap<Usuario, UserDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.IdRolNavigation.Nombre));

        // Formulario de edición
        CreateMap<Usuario, EditUserDto>()
            .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

        // Pantalla de confirmación de eliminación
        CreateMap<Usuario, DeleteUserDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.IdRolNavigation.Nombre));

        //Cateogria
        CreateMap<Categoria, CategoryDto>();
        CreateMap<CreateCategoryDto, Categoria>();
        CreateMap<EditCategoryDto, Categoria>();
        CreateMap<Categoria, EditCategoryDto>();
        CreateMap<Categoria, DeleteCategoryDto>();
    }
}