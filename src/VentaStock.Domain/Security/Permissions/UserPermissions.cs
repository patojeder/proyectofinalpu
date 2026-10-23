namespace VentaStock.Domain.Security.Permissions;

public static class UserPermissions
{
    public const string Read = "Users.Read";
    public const string Create = "Users.Create";
    public const string Update = "Users.Update";
    public const string Delete = "Users.Delete";
    public const string ResetPassword = "Users.ResetPassword";
    public static readonly string[] All = [Read, Create, Update, Delete, ResetPassword];
}
