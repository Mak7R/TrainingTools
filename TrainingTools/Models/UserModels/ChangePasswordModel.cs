using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class ChangePasswordModel
{
    [Required] 
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Incorrect password length")]
    public string CurrentPassword { get; set; }

    [Required] 
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Incorrect password length")]
    public string NewPassword { get; set; }
}