using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class LoginUserModel
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email was empty.")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    [StringLength(User.MaxEmailLength, ErrorMessage = "Incorrect email length")]
    public string Email { get; set; }

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password was empty.")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Incorrect password length")]
    public string Password { get; set; }
}