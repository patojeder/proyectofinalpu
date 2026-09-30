using AutoMapper;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeo de Usuario -> UsuarioSesionDto
        CreateMap<Usuario, UsuarioSesionDto>()
            // IdUsuario, NombreUsuario y Mail se mapean solos por llamarse igual
            // Rol se obtiene del NombreRol de la relación IdRolNavigation
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.IdRolNavigation != null ? src.IdRolNavigation.Nombre : string.Empty))
            // Permisos se proyecta desde la colección IdPermisos
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src =>
                src.IdRolNavigation != null
                    ? src.IdRolNavigation.IdPermisos.Select(p => p.Nombre).ToList()
                    : new List<string>()));
    }
}