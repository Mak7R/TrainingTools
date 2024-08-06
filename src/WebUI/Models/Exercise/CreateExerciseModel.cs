using System.ComponentModel.DataAnnotations;
using Domain.Rules;

namespace WebUI.Models.Exercise;

public class CreateExerciseModel
{
    [Required(ErrorMessage = "Exercise name is required")]
    [StringLength(DataSizes.ExerciseDataSizes.MaxNameSize, MinimumLength = DataSizes.ExerciseDataSizes.MinNameSize,
        ErrorMessage = "Invalid exercise name length")]
    public string? Name { get; set; }

    public Guid GroupId { get; set; }

    [StringLength(DataSizes.ExerciseDataSizes.MaxAboutSize, ErrorMessage = "About length is too large")]
    public string? About { get; set; }
}