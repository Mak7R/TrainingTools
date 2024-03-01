using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models.SharedModels;

public class DeleteModel<TViewModel>
{
    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(User.MaxPasswordLength, ErrorMessage = "Invalid password length")]
    public string Password { get; set; }
    
    [BindNever]
    public TViewModel ViewModel { get; set; } 
}