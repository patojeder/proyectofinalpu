namespace VentaStock.Domain.Security.Permissions;

public static class ProductPermissions
{
    public const string Read = "Products.Read";
    public const string Create = "Products.Create";
    public const string Update = "Products.Update";
    public const string Delete = "Products.Delete";
    public const string ReadCost = "Products.ReadCost";
    public const string ReadSalePrice = "Products.ReadSalePrice";
    public const string UpdatePrices = "Products.UpdatePrices";
    public static readonly string[] All = [Read, Create, Update, Delete, ReadCost, ReadSalePrice, UpdatePrices];
}
