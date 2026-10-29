namespace VentaStock.Domain.Constants;

public static class StockMovementTypes
{
    public const string Receipt = "Receipt";
    public const string Adjustment = "Adjustment";
    public const string Sale = "Sale";
    public const string OpeningBalance = "OpeningBalance";

    public static readonly string[] All = [Receipt, Adjustment, Sale, OpeningBalance];
}
