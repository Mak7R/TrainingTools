using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models;

public class DeleteWorkspaceModel
{
    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Invalid password length")]
    public string Password { get; set; }
    
    [BindNever]
    public string WorkspaceName { get; set; }
}