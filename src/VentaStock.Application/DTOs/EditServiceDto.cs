using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class EditServiceDto : CreateServiceDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}
