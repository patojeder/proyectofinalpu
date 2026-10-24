using System.ComponentModel.DataAnnotations;
using VentaStock.Domain.Constants;
using VentaStock.Domain.Validation;

namespace VentaStock.Application.DTOs;

public class CreateCustomerDto : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido admite hasta 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná el tipo de documento.")]
    [Display(Name = "Tipo de documento")]
    public string DocumentType { get; set; } = DocumentTypes.Dni;

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(30, ErrorMessage = "Revisá el número de documento.")]
    [Display(Name = "Documento")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná el tipo de cliente.")]
    [Display(Name = "Tipo de cliente")]
    public string CustomerType { get; set; } = CustomerTypes.Cash;

    [StringLength(250, ErrorMessage = "La dirección admite hasta 250 caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "El teléfono admite hasta 50 caracteres.")]
    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [StringLength(150, ErrorMessage = "El correo admite hasta 150 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!CustomerDocumentValidator.TryNormalize(DocumentType, DocumentNumber, out _, out var error))
            yield return new ValidationResult(error, [nameof(DocumentNumber)]);
        if (CustomerType is not (CustomerTypes.Cash or CustomerTypes.OnAccount))
            yield return new ValidationResult("Seleccioná contado o cuenta corriente.", [nameof(CustomerType)]);
        if (CustomerType == CustomerTypes.OnAccount && DocumentType is not (DocumentTypes.Cuil or DocumentTypes.Cuit))
            yield return new ValidationResult("Un cliente de cuenta corriente debe tener CUIL o CUIT.", [nameof(DocumentType)]);
    }
}
