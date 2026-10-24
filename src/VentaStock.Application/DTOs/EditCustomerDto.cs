using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class EditCustomerDto : CreateCustomerDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Recargá la pantalla antes de guardar.")]
    public string Version { get; set; } = string.Empty;
}
