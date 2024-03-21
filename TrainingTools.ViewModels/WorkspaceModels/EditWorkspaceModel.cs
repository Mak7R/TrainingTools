using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class EditWorkspaceModel
{
    [Display(Name = "Workspace name")]
    [Required(ErrorMessage = "Name cannot be empty")]
    [StringLength(Workspace.MaxNameLength, ErrorMessage = "Workspace name invalid length")]
    public string Name { get; set; }
    
    [Display(Name = "Access")]
    [Required(ErrorMessage = "Name cannot be empty")]
    public bool IsPublic { get; set; }
}