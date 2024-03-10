using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class UpdateExerciseResultsModel
{
    [Required] 
    public ExerciseResultsObject ExerciseResultsModel { get; set; }
}