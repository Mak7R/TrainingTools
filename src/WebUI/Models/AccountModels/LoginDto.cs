using System.ComponentModel.DataAnnotations;

using static Domain.Rules.DataSizes.ApplicationUser;

namespace WebUI.Models.AccountModels;

public class LoginDto
{
    [Display(Name = "Email or username")]
    [Required(ErrorMessage = "Email or username is required")]
    [StringLength(MaxEmailSize, ErrorMessage = "Email must be shorter")]
    public string? EmailOrUsername { get; set; }

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}