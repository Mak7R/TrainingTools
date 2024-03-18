using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class ExerciseResultsViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("results")]
    public ExerciseResultsObjectViewModel Results { get; set; }

    public ExerciseResultsViewModel(ExerciseResults exerciseResults)
    {
        Id = exerciseResults.Id;
        Results = new ExerciseResultsObjectViewModel(JsonSerializer.Deserialize<ExerciseResultsObject>(exerciseResults.ResultsJson) ?? new ExerciseResultsObject());
    }

    public ExerciseResultsViewModel()
    {
        
    }
}