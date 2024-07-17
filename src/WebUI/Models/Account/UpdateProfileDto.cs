using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using static Domain.Rules.DataSizes.ApplicationUserDataSizes;

namespace WebUI.Models.Account;

public class UpdateProfileDto
{
    [Display(Name = "Nickname")]
    [Required(ErrorMessage = "Nickname is required")]
    [StringLength(MaxUsernameSize, MinimumLength = MinUsernameSize, ErrorMessage = "Nickname length must have less than 64 characters")]
    [Remote(action: "IsUserNameFree", controller: "Account", ErrorMessage = "This username already registered")]
    public string? Username { get; set; } = string.Empty;

    [Display(Name = "About")]
    [StringLength(MaxAboutSize, ErrorMessage = "About must be shorter")]
    public string? About { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    [StringLength(MaxEmailSize, ErrorMessage = "Email must be shorter")]
    [Remote(action: "IsEmailFree", controller: "Account", ErrorMessage = "This email already registered")]
    public string? Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Phone is invalid")]
    public string? Phone { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    public bool IsTrainer { get; set; } = false;
    
    [Display(Name = "New password")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    
    [Display(Name = "Confirm password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Confirm password must be same with password")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
    
    
    [Display(Name = "Current password")]
    [Required(ErrorMessage = "Current password is required for confirm changes")]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }
}