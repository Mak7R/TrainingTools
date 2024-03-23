using System.ComponentModel.DataAnnotations;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class UpdateExerciseResultsModel
{
    [Required] 
    public string ResultsData { get; set; }
}