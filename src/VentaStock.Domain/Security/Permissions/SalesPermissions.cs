namespace VentaStock.Domain.Security.Permissions;

public static class SalesPermissions
{
    public const string Read = "Sales.Read";
    public const string Create = "Sales.Create";
    public static readonly string[] All = [Read, Create];
}
