using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}