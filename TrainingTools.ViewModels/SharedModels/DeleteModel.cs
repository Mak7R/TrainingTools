using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class DeleteModel
{
    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Invalid password length")]
    public string Password { get; set; }
}