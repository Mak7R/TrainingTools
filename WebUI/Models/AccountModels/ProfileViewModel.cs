namespace WebUI.Models.AccountModels;

public class ProfileViewModel
{
    public string? Username { get; set; }
    public string? About { get; set; }
    public IEnumerable<string>? Roles { get; set; }
}