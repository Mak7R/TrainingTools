using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.Models.User;

public class UpdateUserModel
{
    [BindNever] public string? UserName { get; set; }
    public bool SetPrivate { get; set; } = false;
    public bool ClearAbout { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsTrainer { get; set; } = false;
}