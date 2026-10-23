using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Constants;
using VentaStock.Domain.Security.Permissions;

namespace VentaStock.Application.Services;

public class StockService(IStockRepository repository, ICurrentUser currentUser) : IStockService
{
    public async Task<StockIndexDto> GetAllAsync(string? search, bool lowStockOnly)
    {
        if (!currentUser.IsAuthenticated || !StockPermissions.All.Any(currentUser.HasPermission))
            throw new UnauthorizedAccessException();
        search = search?.Trim();
        if (search?.Length > 150)
            throw new InvalidOperationException("La búsqueda admite hasta 150 caracteres.");
        return new StockIndexDto
        {
            Search = search, LowStockOnly = lowStockOnly,
            Items = (await repository.GetAllAsync(search, lowStockOnly)).Select(ToDto).ToList()
        };
    }

    public async Task<StockReceiptDto?> GetForReceiptAsync(int productId)
    {
        RequirePermission(StockPermissions.Receipt);
        var stock = await repository.GetByProductIdAsync(productId);
        return stock is null ? null : new StockReceiptDto
        {
            ProductId = productId, Product = ToDto(stock), OperationId = Guid.NewGuid()
        };
    }

    public async Task<StockAdjustmentDto?> GetForAdjustmentAsync(int productId)
    {
        RequirePermission(StockPermissions.Adjust);
        var stock = await repository.GetByProductIdAsync(productId);
        return stock is null ? null : new StockAdjustmentDto
        {
            ProductId = productId, Product = ToDto(stock), OperationId = Guid.NewGuid(),
            ExpectedQuantity = stock.Quantity
        };
    }

    public async Task<StockHistoryDto?> GetHistoryAsync(int productId, int page)
    {
        RequirePermission(StockPermissions.ReadMovements);
        page = Math.Clamp(page, 1, 100000);
        var stock = await repository.GetByProductIdAsync(productId);
        if (stock is null) return null;
        const int pageSize = 50;
        var movements = await repository.GetMovementsAsync(productId, (page - 1) * pageSize, pageSize + 1);
        return new StockHistoryDto
        {
            Product = ToDto(stock), Page = page, HasNextPage = movements.Count > pageSize,
            Movements = movements.Take(pageSize).Select(m => new StockMovementDto
            {
                Id = m.Id,
                Type = m.MovementType switch
                {
                    StockMovementTypes.Receipt => "Ingreso",
                    StockMovementTypes.Adjustment => "Ajuste",
                    StockMovementTypes.OpeningBalance => "Saldo inicial",
                    _ => "Movimiento"
                },
                QuantityChange = m.QuantityChange, QuantityBefore = m.QuantityBefore, QuantityAfter = m.QuantityAfter,
                UserName = m.UserName, Reason = m.Reason, CreatedAtUtc = m.CreatedAtUtc
            }).ToList()
        };
    }

    public Task<bool> ReceiveAsync(StockReceiptDto dto)
    {
        RequirePermission(StockPermissions.Receipt);
        ValidateOperation(dto);
        if (!dto.Quantity.HasValue || dto.Quantity <= 0)
            throw new InvalidOperationException("La cantidad a ingresar debe ser un entero mayor a 0.");
        var reason = NormalizeReason(dto.Reason);
        return repository.ReceiveAsync(dto.ProductId, dto.Quantity.Value, GetUserId(), dto.OperationId, reason);
    }

    public Task<bool> AdjustAsync(StockAdjustmentDto dto)
    {
        RequirePermission(StockPermissions.Adjust);
        ValidateOperation(dto);
        if (!dto.CountedQuantity.HasValue || dto.CountedQuantity < 0 || !dto.ExpectedQuantity.HasValue)
            throw new InvalidOperationException("Indicá la cantidad real contada y recargá la pantalla si cambió el stock.");
        var reason = NormalizeReason(dto.Reason);
        if (reason is null)
            throw new InvalidOperationException("El motivo del ajuste es obligatorio.");
        return repository.AdjustAsync(dto.ProductId, dto.CountedQuantity.Value, dto.ExpectedQuantity.Value,
            GetUserId(), dto.OperationId, reason);
    }

    private static StockItemDto ToDto(ProductStock stock) => new()
    {
        ProductId = stock.ProductId, Name = stock.Product.Name, Barcode = stock.Product.Barcode,
        Sector = stock.Product.Sector, Shelf = stock.Product.Shelf,
        Quantity = stock.Quantity, MinimumQuantity = stock.MinimumQuantity
    };

    private static string? NormalizeReason(string? reason)
    {
        reason = reason?.Trim();
        if (reason?.Length > 500)
            throw new InvalidOperationException("El motivo u observación no puede superar los 500 caracteres.");
        return string.IsNullOrWhiteSpace(reason) ? null : reason;
    }

    private static void ValidateOperation(StockOperationDto dto)
    {
        if (dto.ProductId <= 0 || dto.OperationId == Guid.Empty)
            throw new InvalidOperationException("La operación no es válida. Volvé a abrir el formulario.");
    }

    private int GetUserId() => currentUser.UserId ?? throw new UnauthorizedAccessException();

    private void RequirePermission(string permission)
    {
        if (!currentUser.IsAuthenticated || !currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }
}
