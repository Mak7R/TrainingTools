using System.Text.Json.Serialization;

namespace TrainingTools.ViewModels;

public class ErrorViewModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ErrorViewModel(string message)
    {
        Message = message;
    }
}