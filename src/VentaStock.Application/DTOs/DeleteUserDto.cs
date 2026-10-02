namespace VentaStock.Application.DTOs;

public class DeleteUserDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
}