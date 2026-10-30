namespace VentaStock.Application.DTOs;

public class UserSessionDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public string CredentialVersion { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public List<int> BranchIds { get; set; } = new();
}
