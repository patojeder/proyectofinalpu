using System;
using System.Collections.Generic;

namespace VentaStock.Domain.Entities;

public partial class RefreshToken
{
    public int IdRefreshToken { get; set; }

    public int IdUsuario { get; set; }

    public string Token { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public bool Revocado { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
