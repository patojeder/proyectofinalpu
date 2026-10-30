using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Constants;
using VentaStock.Domain.Security.Permissions;

namespace VentaStock.Application.Services;

public class StockService(IStockRepository repository, ICurrentUser currentUser) : IStockService
{
    public async Task<StockIndexDto> GetAllAsync(int? branchId, string? search, bool lowStockOnly)
    {
        if (!currentUser.IsAuthenticated || !StockPermissions.All.Any(currentUser.HasPermission))
            throw new UnauthorizedAccessException();
        search = search?.Trim();
        if (search?.Length > 150)
            throw new InvalidOperationException("La búsqueda admite hasta 150 caracteres.");

        var branches = (await repository.GetBranchesAsync())
            .Select(b => new BranchSelectDto { Id = b.Id, Name = b.Name })
            .ToList();

        int? selectedBranchId = branchId.HasValue && branches.Any(b => b.Id == branchId.Value)
            ? branchId.Value
            : branches.FirstOrDefault(b => currentUser.CanAccessBranch(b.Id))?.Id ?? branches.FirstOrDefault()?.Id;

        var items = selectedBranchId.HasValue
            ? (await repository.GetAllByBranchAsync(selectedBranchId.Value, search, lowStockOnly)).Select(ToDto).ToList()
            : [];

        return new StockIndexDto
        {
            BranchId = selectedBranchId,
            CanOperateInSelectedBranch = selectedBranchId.HasValue && currentUser.CanAccessBranch(selectedBranchId.Value),
            Search = search,
            LowStockOnly = lowStockOnly,
            Branches = branches,
            Items = items
        };
    }

    public async Task<StockReceiptDto?> GetForReceiptAsync(int productId, int branchId)
    {
        RequirePermission(StockPermissions.Receipt);
        RequireBranchAccess(branchId);
        var stock = await repository.GetByProductAndBranchAsync(productId, branchId);
        return stock is null ? null : new StockReceiptDto
        {
            ProductId = productId,
            BranchId = branchId,
            Product = ToDto(stock),
            OperationId = Guid.NewGuid()
        };
    }

    public async Task<StockAdjustmentDto?> GetForAdjustmentAsync(int productId, int branchId)
    {
        RequirePermission(StockPermissions.Adjust);
        RequireBranchAccess(branchId);
        var stock = await repository.GetByProductAndBranchAsync(productId, branchId);
        return stock is null ? null : new StockAdjustmentDto
        {
            ProductId = productId,
            BranchId = branchId,
            Product = ToDto(stock),
            OperationId = Guid.NewGuid(),
            ExpectedQuantity = stock.Quantity
        };
    }

    public async Task<StockSettingsDto?> GetForSettingsAsync(int productId, int branchId)
    {
        RequirePermission(StockPermissions.Adjust);
        RequireBranchAccess(branchId);
        var stock = await repository.GetByProductAndBranchAsync(productId, branchId);
        return stock is null ? null : new StockSettingsDto
        {
            ProductId = productId,
            BranchId = branchId,
            Product = ToDto(stock),
            Sector = stock.Sector,
            Shelf = stock.Shelf,
            MinimumQuantity = stock.MinimumQuantity
        };
    }

    public async Task<StockHistoryDto?> GetHistoryAsync(int productId, int branchId, StockHistoryFilterDto? filter, int page)
    {
        RequirePermission(StockPermissions.ReadMovements);
        RequireBranchAccess(branchId);
        page = Math.Clamp(page, 1, 100000);
        var stock = await repository.GetByProductAndBranchAsync(productId, branchId);
        if (stock is null) return null;

        var normalizedFilter = new StockHistoryFilterDto
        {
            FromDate = filter?.FromDate,
            ToDate = filter?.ToDate,
            UserName = string.IsNullOrWhiteSpace(filter?.UserName) ? null : filter.UserName.Trim(),
            MovementType = string.IsNullOrWhiteSpace(filter?.MovementType) ? null : filter.MovementType.Trim()
        };

        if (normalizedFilter.UserName?.Length > 50)
            throw new InvalidOperationException("El filtro de usuario admite hasta 50 caracteres.");

        if (normalizedFilter.MovementType is not null && !StockMovementTypes.All.Contains(normalizedFilter.MovementType))
            throw new InvalidOperationException("El tipo de movimiento seleccionado no es válido.");

        if (normalizedFilter.FromDate.HasValue && normalizedFilter.ToDate.HasValue
            && normalizedFilter.FromDate.Value > normalizedFilter.ToDate.Value)
            throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");

        const int pageSize = 50;
        var movements = await repository.GetMovementsAsync(
            productId,
            branchId,
            normalizedFilter.FromDate,
            normalizedFilter.ToDate,
            normalizedFilter.UserName,
            normalizedFilter.MovementType,
            (page - 1) * pageSize,
            pageSize + 1);

        return new StockHistoryDto
        {
            Product = ToDto(stock),
            Filter = normalizedFilter,
            Page = page,
            HasNextPage = movements.Count > pageSize,
            Movements = movements.Take(pageSize).Select(m => new StockMovementDto
            {
                Id = m.Id,
                Type = m.MovementType switch
                {
                    StockMovementTypes.Receipt => "Ingreso",
                    StockMovementTypes.Adjustment => "Ajuste",
                    StockMovementTypes.Sale => "Venta",
                    StockMovementTypes.OpeningBalance => "Saldo inicial",
                    _ => "Movimiento"
                },
                QuantityChange = m.QuantityChange,
                QuantityBefore = m.QuantityBefore,
                QuantityAfter = m.QuantityAfter,
                UserName = m.UserName,
                Reason = m.Reason,
                CreatedAtUtc = m.CreatedAtUtc
            }).ToList()
        };
    }

    public Task<bool> ReceiveAsync(StockReceiptDto dto)
    {
        RequirePermission(StockPermissions.Receipt);
        ValidateOperation(dto);
        RequireBranchAccess(dto.BranchId);
        if (!dto.Quantity.HasValue || dto.Quantity <= 0)
            throw new InvalidOperationException("La cantidad a ingresar debe ser un entero mayor a 0.");
        var reason = NormalizeReason(dto.Reason);
        return repository.ReceiveAsync(dto.ProductId, dto.BranchId, dto.Quantity.Value, GetUserId(), dto.OperationId, reason);
    }

    public Task<bool> AdjustAsync(StockAdjustmentDto dto)
    {
        RequirePermission(StockPermissions.Adjust);
        ValidateOperation(dto);
        RequireBranchAccess(dto.BranchId);
        if (!dto.CountedQuantity.HasValue || dto.CountedQuantity < 0 || !dto.ExpectedQuantity.HasValue)
            throw new InvalidOperationException("Indicá la cantidad real contada y recargá la pantalla si cambió el stock.");
        var reason = NormalizeReason(dto.Reason);
        if (reason is null)
            throw new InvalidOperationException("El motivo del ajuste es obligatorio.");
        return repository.AdjustAsync(dto.ProductId, dto.BranchId, dto.CountedQuantity.Value, dto.ExpectedQuantity.Value,
            GetUserId(), dto.OperationId, reason);
    }

    public Task UpdateSettingsAsync(StockSettingsDto dto)
    {
        RequirePermission(StockPermissions.Adjust);
        if (dto.ProductId <= 0 || dto.BranchId <= 0)
            throw new InvalidOperationException("Seleccioná un producto y una sucursal válidos.");
        RequireBranchAccess(dto.BranchId);

        var sector = string.IsNullOrWhiteSpace(dto.Sector) ? null : dto.Sector.Trim();
        var shelf = string.IsNullOrWhiteSpace(dto.Shelf) ? null : dto.Shelf.Trim();
        if (sector?.Length > 50 || shelf?.Length > 50)
            throw new InvalidOperationException("Revisá la longitud del sector y estante (máximo 50 caracteres).");

        var minimumQuantity = dto.MinimumQuantity ?? 0;
        if (minimumQuantity < 0)
            throw new InvalidOperationException("El stock mínimo debe ser mayor o igual a 0.");

        return repository.UpdateSettingsAsync(dto.ProductId, dto.BranchId, sector, shelf, minimumQuantity);
    }

    private static StockItemDto ToDto(ProductStock stock) => new()
    {
        ProductId = stock.ProductId,
        BranchId = stock.BranchId,
        BranchName = stock.Branch.Name,
        Name = stock.Product.Name,
        Barcode = stock.Product.Barcode,
        Sector = stock.Sector,
        Shelf = stock.Shelf,
        Quantity = stock.Quantity,
        MinimumQuantity = stock.MinimumQuantity
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
        if (dto.ProductId <= 0 || dto.BranchId <= 0 || dto.OperationId == Guid.Empty)
            throw new InvalidOperationException("La operación no es válida. Volvé a abrir el formulario.");
    }

    private int GetUserId() => currentUser.UserId ?? throw new UnauthorizedAccessException();

    private void RequirePermission(string permission)
    {
        if (!currentUser.IsAuthenticated || !currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }

    private void RequireBranchAccess(int branchId)
    {
        if (!currentUser.CanAccessBranch(branchId))
            throw new UnauthorizedAccessException();
    }
}
