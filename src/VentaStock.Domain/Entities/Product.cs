using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Barcode { get; set; } = null!;

    public string? Sector { get; set; }

    public string? Shelf { get; set; }

    public decimal? CostPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public virtual ProductStock? ProductStock { get; set; }

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
