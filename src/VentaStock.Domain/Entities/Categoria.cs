using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class Categoria
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }
}
