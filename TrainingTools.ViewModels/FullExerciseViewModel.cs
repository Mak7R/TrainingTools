using System.Text.Json.Serialization;
using Contracts.Models;
using TrainingTools.Models;

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
    [JsonPropertyName("results")]
    public ExerciseResultsViewModel? Results { get; set; }
    
    public FullExerciseViewModel(Exercise exercise, ExerciseResults? results)
    {
        Id = exercise.Id;
        Name = exercise.Name;
        Workspace = new WorkspaceViewModel(exercise.Workspace);
        Group = exercise.Group == null ? null : new GroupViewModel(exercise.Group);
        Results = results == null ? null : new ExerciseResultsViewModel(results);
    }

    public FullExerciseViewModel()
    {
        
    }
}