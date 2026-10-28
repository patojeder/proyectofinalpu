namespace VentaStock.Application.DTOs;

public class UserSessionDto
{
	public int Id { get; set; }
	public string UserName { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public List<string> Permissions { get; set; } = new();
}
