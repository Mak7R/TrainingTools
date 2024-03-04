using System.Collections;
using System.Text.Json;
using Contracts.Models;

namespace TrainingTools.Models;

public class ExerciseResultsViewModel
{
    public Guid Id { get; set; }
    public ExerciseResultsObject Results { get; set; }

    public ExerciseResultsViewModel(ExerciseResults exerciseResults)
    {
        Id = exerciseResults.Id;
        Results = JsonSerializer.Deserialize<ExerciseResultsObject>(exerciseResults.ResultsJson) ?? new ExerciseResultsObject();
    }
}