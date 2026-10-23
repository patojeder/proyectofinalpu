using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class StockMovement
{
    public long Id { get; set; }

    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string MovementType { get; set; } = null!;

    public long QuantityChange { get; set; }

    public int QuantityBefore { get; set; }

    public int QuantityAfter { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Guid OperationId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? User { get; set; }
}
