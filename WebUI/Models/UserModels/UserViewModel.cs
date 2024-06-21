namespace WebUI.Models.UserModels;

public class UserViewModel
{
    public string? Username { get; set; }
    public string? About { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPublic { get; set; }
}