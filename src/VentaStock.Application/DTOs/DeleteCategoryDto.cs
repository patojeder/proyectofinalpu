namespace VentaStock.Application.DTOs;

public class DeleteCategoryDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}