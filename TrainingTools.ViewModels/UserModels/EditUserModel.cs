using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class EditUserModel
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
}