using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
