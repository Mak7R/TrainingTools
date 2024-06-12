namespace WebUI.Models.AccountModels;

public class ProfileViewModel
{
    public string? Username { get; set; }
    public string? About { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPublic { get; set; }
    
}