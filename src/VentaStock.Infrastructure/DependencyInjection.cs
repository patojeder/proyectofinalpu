using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VentaStock.Domain.Contracts;
using VentaStock.Infrastructure.Data;
using VentaStock.Infrastructure.Repositories;

namespace VentaStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VentaStockDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("VentaStock")));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}
