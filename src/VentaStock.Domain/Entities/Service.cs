using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
