namespace WebUI.Models.Account;

public class ChangePasswordModel
{
    public string OldPassword { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}