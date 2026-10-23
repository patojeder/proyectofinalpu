namespace VentaStock.Application.DTOs;

public class DeleteCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}