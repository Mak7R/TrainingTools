using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ExerciseModels;

public class UpdateExerciseModel
{
    [Required(ErrorMessage = "Exercise name is required")]
    [StringLength(Domain.Rules.DataSizes.ExerciseDataSizes.MaxNameSize, MinimumLength = Domain.Rules.DataSizes.ExerciseDataSizes.MinNameSize, ErrorMessage = "Invalid exercise name length")]
    public string? Name { get; set; }
    public Guid GroupId { get; set; }
    
    [StringLength(Domain.Rules.DataSizes.ExerciseDataSizes.MaxAboutSize, ErrorMessage = "About length is too large")] 
    public string? About { get; set; }
}