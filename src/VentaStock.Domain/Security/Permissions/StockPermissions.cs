namespace VentaStock.Domain.Security.Permissions;

public static class StockPermissions
{
    public const string Read = "Stock.Read";
    public static readonly string[] All = [Read];
}
