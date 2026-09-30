using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VentaStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registrar el DbContext generado y los repositorios al implementarlos.
        // La conexión se obtendrá de configuration.GetConnectionString("VentaStock").
        return services;
    }
}
