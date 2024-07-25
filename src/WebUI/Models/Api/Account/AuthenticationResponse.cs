namespace WebUI.Models.Api.Account;

public class AuthenticationResponse
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public string? Token { get; set; }
}