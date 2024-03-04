using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.Models;

public class UpdateExerciseResultsModel
{
    public Guid ExerciseResultsId { get; set; }
    
    [Required]
    public ExerciseResultsObject ExerciseResultsModel { get; set; }

    
}