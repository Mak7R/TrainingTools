using System.Text.Json.Serialization;

namespace TrainingTools.ViewModels;

public class ExerciseResultsViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("owner")]
    public PublicUserViewModel Owner { get; set; }
    
    [JsonPropertyName("data")]
    public string Data { get; set; }

    public ExerciseResultsViewModel(Guid id, PublicUserViewModel owner, string data)
    {
        Id = id;
        Owner = owner;
        Data = data;
    }

    public ExerciseResultsViewModel()
    {
        
    }
}