using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class AddWorkspaceModel
{
    [Display(Name = "Workspace name")]
    [Required(ErrorMessage = "Name cannot be empty")]
    [StringLength(Workspace.MaxNameLength, ErrorMessage = "Workspace name invalid length")]
    public string Name { get; set; }
}