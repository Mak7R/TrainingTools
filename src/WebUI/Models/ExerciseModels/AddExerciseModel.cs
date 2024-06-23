using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ExerciseModels;

public class AddExerciseModel
{
    [Required(ErrorMessage = "Exercise name is required")]
    [StringLength(Domain.Rules.DataSizes.Exercise.MaxNameSize, MinimumLength = Domain.Rules.DataSizes.Exercise.MinNameSize, ErrorMessage = "Invalid exercise name length")]
    public string? Name { get; set; }
    public Guid GroupId { get; set; }
    
    [StringLength(Domain.Rules.DataSizes.Exercise.MaxAboutSize, ErrorMessage = "About length is too large")] 
    public string? About { get; set; }
}