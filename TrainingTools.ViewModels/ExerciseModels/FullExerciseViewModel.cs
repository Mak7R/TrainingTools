using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class FullExerciseViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("workspace")]
    public WorkspaceViewModel Workspace { get; set; }
    [JsonPropertyName("group")]
    public GroupViewModel? Group { get; set; }
    [JsonPropertyName("userresults")]
    public ExerciseResultsViewModel? Results { get; set; }
    
    [JsonPropertyName("allresults")]
    public IEnumerable<ExerciseResultsViewModel> AllResults { get; set; }
    
    public FullExerciseViewModel(Guid id, string name, WorkspaceViewModel workspace, GroupViewModel? group, ExerciseResultsViewModel? results, IEnumerable<ExerciseResultsViewModel> allResults)
    {
        Id = id;
        Name = name;
        Workspace = workspace;
        Group = group;
        Results = results;
        AllResults = allResults;
    }

    public FullExerciseViewModel()
    {
        
    }
}