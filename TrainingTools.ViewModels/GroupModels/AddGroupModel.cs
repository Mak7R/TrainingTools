using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class AddGroupModel
{
    [Display(Name = "Group name")]
    [Required(ErrorMessage = "Name cannot be empty")]
    [StringLength(Exercise.MaxNameLength, ErrorMessage = "Group name has invalid length")]
    public string Name { get; set; }
}