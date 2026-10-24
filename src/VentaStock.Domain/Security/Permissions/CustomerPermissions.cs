namespace VentaStock.Domain.Security.Permissions;

public static class CustomerPermissions
{
    public const string Read = "Customers.Read";
    public const string Create = "Customers.Create";
    public const string CreateOnAccount = "Customers.CreateOnAccount";
    public const string Update = "Customers.Update";
    public static readonly string[] All = [Read, Create, CreateOnAccount, Update];
}
