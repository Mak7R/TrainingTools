using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class DeleteUserModel
{
    [Required(ErrorMessage = "Password was empty.")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Incorrect password length")]
    public string Password { get; set; }
}