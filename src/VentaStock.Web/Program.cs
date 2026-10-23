using VentaStock.Domain.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using VentaStock.Application;
using VentaStock.Infrastructure;
using VentaStock.Application.Contracts;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Web.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";           
        options.LogoutPath = "/Auth/Logout";         
        options.AccessDeniedPath = "/Auth/Denegado"; 
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;            
        options.Cookie.HttpOnly = true;              
        options.Cookie.Name = "VentaStock.Session";  
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in UserPermissions.All.Concat(CategoryPermissions.All).Concat(ServicePermissions.All).Concat(SalesPermissions.All))
    {
        options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser()
            .RequireAssertion(context => context.User.IsInRole(SystemRoles.Owner)
                || context.User.HasClaim("Permission", permission)));
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
