using System.Text.Json.Serialization;
using Contracts.Models;
using TrainingTools.Models;

namespace TrainingTools.ViewModels;

public class FullWorkspaceViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("owner")]
    public UserViewModel Owner { get; set; }
    [JsonPropertyName("groups")]
    public IEnumerable<GroupViewModel> Groups { get; set; }
    [JsonPropertyName("exercises")]
    public IEnumerable<ExerciseViewModel> Exercises { get; set; }
    
    public FullWorkspaceViewModel(Workspace workspace, IEnumerable<GroupViewModel> groups, IEnumerable<ExerciseViewModel> exercises)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        Owner = new UserViewModel(workspace.Owner);
        Groups = groups;
        Exercises = exercises;
    }

    public FullWorkspaceViewModel()
    {
        
    }
}