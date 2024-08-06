namespace WebUI.Models.Account;

public class ProfileViewModel
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? About { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPublic { get; set; }
}