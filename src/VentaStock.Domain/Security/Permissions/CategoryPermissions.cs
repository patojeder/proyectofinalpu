namespace VentaStock.Domain.Security.Permissions;

public static class CategoryPermissions
{
    public const string Read = "Categories.Read";
    public const string Create = "Categories.Create";
    public const string Update = "Categories.Update";
    public const string Delete = "Categories.Delete";
    public static readonly string[] All = [Read, Create, Update, Delete];
}
