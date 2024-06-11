namespace WebUI.Models.AccountModels;

public class FullProfileViewModel : ProfileViewModel
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPublic { get; set; }
}