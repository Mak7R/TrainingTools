using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class ExerciseResultsViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("owner")]
    public PublicUserViewModel Owner { get; set; }
    
    [JsonPropertyName("results")]
    public ExerciseResultsObjectViewModel Results { get; set; }

    public ExerciseResultsViewModel(Guid id, PublicUserViewModel owner, ExerciseResultsObjectViewModel results)
    {
        Id = id;
        Owner = owner;
        Results = results;
    }

    public ExerciseResultsViewModel()
    {
        
    }
}