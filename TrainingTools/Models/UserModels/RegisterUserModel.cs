using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class RegisterUserModel
{
  
    [Display(Name = "Username")]
    [Required(ErrorMessage = "User name was empty")]
    [StringLength(User.MaxNameLength, ErrorMessage = "Incorrect name length")]
    public string Name { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email was empty")]
    [EmailAddress(ErrorMessage = "Email was invalid")]
    [StringLength(User.MaxEmailLength, ErrorMessage = $"Incorrect email length")]
    public string Email { get; set; }

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password was empty")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Incorrect password length")]
    public string Password { get; set; }
    
    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "Confirm password was empty")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Invalid confirm password length")]
    [Compare(nameof(Password), ErrorMessage = "Confirm password must be same with password")]
    public string ConfirmPassword { get; set; }
}