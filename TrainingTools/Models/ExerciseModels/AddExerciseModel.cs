using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class AddExerciseModel
{
    [Display(Name = "Exercise name")]
    [Required(ErrorMessage = "Name cannot be empty")]
    [StringLength(Exercise.MaxNameLength, ErrorMessage = "Exercise name invalid length")]
    public string Name { get; set; }
}