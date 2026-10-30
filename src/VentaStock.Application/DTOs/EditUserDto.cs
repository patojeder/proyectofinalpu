using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class EditUserDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [Display(Name = "Nombre de Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un rol.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido.")]
    [Display(Name = "Rol")]
    public int RoleId { get; set; }

    [Display(Name = "Sucursales en las que trabaja")]
    public List<int> BranchIds { get; set; } = [];

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva Contraseña (opcional)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar Nueva Contraseña")]
    public string? ConfirmPassword { get; set; }
}
