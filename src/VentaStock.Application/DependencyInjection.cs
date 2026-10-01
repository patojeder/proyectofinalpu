using Microsoft.Extensions.DependencyInjection;
using VentaStock.Application.Contracts;
using VentaStock.Application.Mappings;
using VentaStock.Application.Services;

namespace VentaStock.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}