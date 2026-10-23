using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class ProductStock
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int MinimumQuantity { get; set; }

    public virtual Product Product { get; set; } = null!;
}
