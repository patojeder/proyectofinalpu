using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [Display(Name = "Nombre de Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un rol.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido.")]
    [Display(Name = "Rol")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas ingresadas no coinciden.")]
    [Display(Name = "Confirmar Contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
