namespace VentaStock.Domain.Security.Permissions;

public static class StockPermissions
{
    public const string Read = "Stock.Read";
    public const string Receipt = "Stock.Receipt";
    public const string Adjust = "Stock.Adjust";
    public const string ReadMovements = "Stock.ReadMovements";
    public static readonly string[] All = [Read, Receipt, Adjust, ReadMovements];
}
