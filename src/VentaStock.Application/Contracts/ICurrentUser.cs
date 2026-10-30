namespace VentaStock.Application.Contracts;

public interface ICurrentUser
{
    int? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsOwner { get; }
    IReadOnlyCollection<int> BranchIds { get; }
    bool HasPermission(string permission);
    bool CanAccessBranch(int branchId);
}
