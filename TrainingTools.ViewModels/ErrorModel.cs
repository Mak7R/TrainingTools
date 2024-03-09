using System.Text.Json.Serialization;

namespace TrainingTools.ViewModels;

public class ErrorModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ErrorModel(string message)
    {
        Message = message;
    }
}