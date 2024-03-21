using System.Text.Json.Serialization;

namespace Contracts.Models;

public class Entry
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("weight")]
    public int Weight { get; set; }
}

public class ExerciseResultsObject : List<Entry>
{
    
}