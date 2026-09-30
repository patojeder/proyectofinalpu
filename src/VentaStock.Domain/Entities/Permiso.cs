using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
