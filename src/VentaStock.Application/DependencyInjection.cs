using Microsoft.Extensions.DependencyInjection;

namespace VentaStock.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar los servicios de aplicación con AddScoped al implementarlos.
        return services;
    }
}
