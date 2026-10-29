using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Barcode { get; set; } = null!;

    public decimal? CostPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public virtual ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
