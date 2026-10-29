using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class ProductStock
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int BranchId { get; set; }

    public int Quantity { get; set; }

    public int MinimumQuantity { get; set; }

    public string? Sector { get; set; }

    public string? Shelf { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
