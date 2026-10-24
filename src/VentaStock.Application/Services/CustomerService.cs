using System.ComponentModel.DataAnnotations;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Constants;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Domain.Validation;

namespace VentaStock.Application.Services;

public class CustomerService(ICustomerRepository repository, ICurrentUser currentUser) : ICustomerService
{
    public async Task<CustomerIndexDto> GetAllAsync(string? search)
    {
        RequirePermission(CustomerPermissions.Read);
        search = search?.Trim();
        if (search?.Length > 150)
            throw new InvalidOperationException("La búsqueda admite hasta 150 caracteres.");
        return new CustomerIndexDto
        {
            Search = search,
            Items = (await repository.GetAllAsync(search)).Select(c => new CustomerDto
            {
                Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, DocumentType = c.DocumentType,
                DocumentNumber = c.DocumentNumber, CustomerType = c.CustomerType,
                Address = c.Address, Phone = c.Phone, Email = c.Email
            }).ToList()
        };
    }

    public async Task<EditCustomerDto?> GetForEditAsync(int id)
    {
        RequirePermission(CustomerPermissions.Update);
        var customer = await repository.GetByIdAsync(id);
        return customer is null ? null : new EditCustomerDto
        {
            Id = customer.Id, FirstName = customer.FirstName, LastName = customer.LastName,
            DocumentType = customer.DocumentType, DocumentNumber = customer.DocumentNumber,
            CustomerType = customer.CustomerType, Address = customer.Address, Phone = customer.Phone,
            Email = customer.Email, Version = Convert.ToBase64String(customer.Version)
        };
    }

    public async Task CreateAsync(CreateCustomerDto dto)
    {
        RequirePermission(CustomerPermissions.Create);
        if (dto.CustomerType == CustomerTypes.OnAccount)
            RequirePermission(CustomerPermissions.CreateOnAccount);
        var customer = new Customer();
        await ApplyFieldsAsync(customer, dto);
        await repository.CreateAsync(customer);
    }

    public async Task UpdateAsync(EditCustomerDto dto)
    {
        RequirePermission(CustomerPermissions.Update);
        var customer = await repository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("El cliente ya no existe.");
        if (dto.CustomerType == CustomerTypes.OnAccount && customer.CustomerType != CustomerTypes.OnAccount)
            RequirePermission(CustomerPermissions.CreateOnAccount);
        byte[] version;
        try { version = Convert.FromBase64String(dto.Version ?? ""); }
        catch (FormatException) { throw new InvalidOperationException("Recargá la pantalla antes de guardar."); }
        if (version.Length != 8)
            throw new InvalidOperationException("Recargá la pantalla antes de guardar.");
        await ApplyFieldsAsync(customer, dto);
        await repository.UpdateAsync(customer, version);
    }

    private async Task ApplyFieldsAsync(Customer customer, CreateCustomerDto dto)
    {
        var firstName = dto.FirstName?.Trim();
        var lastName = dto.LastName?.Trim();
        if (string.IsNullOrEmpty(firstName) || firstName.Length > 100
            || string.IsNullOrEmpty(lastName) || lastName.Length > 100)
            throw new InvalidOperationException("Nombre y apellido son obligatorios y admiten hasta 100 caracteres cada uno.");
        if (!CustomerDocumentValidator.TryNormalize(dto.DocumentType, dto.DocumentNumber, out var number, out var error))
            throw new InvalidOperationException(error);
        if (dto.CustomerType is not (CustomerTypes.Cash or CustomerTypes.OnAccount))
            throw new InvalidOperationException("Seleccioná contado o cuenta corriente.");
        if (dto.CustomerType == CustomerTypes.OnAccount && dto.DocumentType is not (DocumentTypes.Cuil or DocumentTypes.Cuit))
            throw new InvalidOperationException("Un cliente de cuenta corriente debe tener CUIL o CUIT.");
        if (await repository.ExistsByDocumentAsync(number, customer.Id))
            throw new InvalidOperationException("Ya existe un cliente con ese documento.");
        var address = NormalizeOptional(dto.Address, 250);
        var phone = NormalizeOptional(dto.Phone, 50);
        var email = NormalizeOptional(dto.Email, 150);
        if (phone is not null && !new PhoneAttribute().IsValid(phone))
            throw new InvalidOperationException("El teléfono no tiene un formato válido.");
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("El correo electrónico no tiene un formato válido.");
        customer.FirstName = firstName; customer.LastName = lastName;
        customer.DocumentType = dto.DocumentType; customer.DocumentNumber = number;
        customer.CustomerType = dto.CustomerType; customer.Address = address;
        customer.Phone = phone; customer.Email = email;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        value = value?.Trim();
        if (value?.Length > maxLength)
            throw new InvalidOperationException("Revisá la longitud de los datos de contacto.");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void RequirePermission(string permission)
    {
        if (!currentUser.IsAuthenticated || !currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }
}
