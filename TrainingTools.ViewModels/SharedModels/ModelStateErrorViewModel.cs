using System.Text.Json.Serialization;

namespace TrainingTools.ViewModels;

public class ModelStateErrorViewModel
{
    [JsonPropertyName("errors")]
    public Dictionary<string, IEnumerable<string>?> Errors { get; set; }
    
    public ModelStateErrorViewModel(Dictionary<string, IEnumerable<string>?> errors)
    {
        Errors = errors;
    }

    public ModelStateErrorViewModel()
    {
        
    }
}