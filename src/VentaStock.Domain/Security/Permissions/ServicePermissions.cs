namespace VentaStock.Domain.Security.Permissions;

public static class ServicePermissions
{
    public const string Read = "Services.Read";
    public const string Create = "Services.Create";
    public const string Update = "Services.Update";
    public const string Delete = "Services.Delete";
    public static readonly string[] All = [Read, Create, Update, Delete];
}
