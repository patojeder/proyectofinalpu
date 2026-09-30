using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VentaStockDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("VentaStock")));

        return services;
    }
}