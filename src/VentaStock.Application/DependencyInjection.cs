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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
