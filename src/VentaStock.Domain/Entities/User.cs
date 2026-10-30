using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool MustChangePassword { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
