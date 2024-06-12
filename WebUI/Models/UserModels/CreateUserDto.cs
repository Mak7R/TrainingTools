using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using static Domain.Rules.DataSizes.ApplicationUser;

namespace WebUI.Models.UserModels;

public class CreateUserDto
{
    [Display(Name = "Nickname")]
    [Required(ErrorMessage = "Nickname is required")]
    [StringLength(MaxUsernameSize, MinimumLength = MinUsernameSize, ErrorMessage = "Nickname length must have less than 64 characters")]
    [Remote(action: "IsUserNameFree", controller: "Account", ErrorMessage = "This username already registered")]
    public string? Username { get; set; }
    
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    [StringLength(MaxEmailSize, ErrorMessage = "Email must be shorter")]
    [Remote(action: "IsEmailFree", controller: "Account", ErrorMessage = "This email already registered")]
    public string? Email { get; set; }
    
    [Display(Name = "Phone number")]
    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Phone is invalid")]
    public string? Phone { get; set; }
    
    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    
    [Display(Name = "Confirm password")]
    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(Password), ErrorMessage = "Confirm password must be same with password")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
    
    public bool IsPublic { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
}