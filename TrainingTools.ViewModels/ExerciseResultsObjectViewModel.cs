using System.Text.Json.Serialization;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class ResultsEntryViewModel
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("weight")]
    public int Weight { get; set; }
}

public class ExerciseResultsObjectViewModel : List<ResultsEntryViewModel>
{
    public ExerciseResultsObjectViewModel(ExerciseResultsObject resultsObject) 
        : base(resultsObject.Select(entry => new ResultsEntryViewModel{Count = entry.Count, Weight = entry.Weight}))
    {
        
    }

    public ExerciseResultsObjectViewModel()
    {
        
    }
}