namespace VentaStock.Application.DTOs;

public class UsuarioSesionDto
{
	public int IdUsuario { get; set; }
	public string NombreUsuario { get; set; } = string.Empty;
	public string Mail { get; set; } = string.Empty;
	public string Rol { get; set; } = string.Empty;
	public List<string> Permisos { get; set; } = new();
}