using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using static Domain.Rules.DataSizes.ApplicationUserDataSizes;

namespace WebUI.Models.UserModels;

public class UpdateUserDto
{   
    [Display(Name = "Nickname")]
    [Required(ErrorMessage = "Nickname is required")]
    [StringLength(MaxUsernameSize, MinimumLength = MinUsernameSize, ErrorMessage = "Nickname length must have less than 64 characters")]
    [Remote(action: "IsUserNameFree", controller: "Account", ErrorMessage = "This username already registered")]
    public string? UpdateUsername { get; set; } = null;
    
    public bool SetPrivate { get; set; } = false;
    public bool ClearAbout { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsTrainer { get; set; } = false;
}