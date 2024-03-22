using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class FullWorkspaceViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("owner")]
    public PublicUserViewModel Owner { get; set; }
    [JsonPropertyName("groups")]
    public IEnumerable<GroupViewModel> Groups { get; set; }
    [JsonPropertyName("exercises")]
    public IEnumerable<ExerciseViewModel> Exercises { get; set; }
    
    [JsonPropertyName("permission")]
    public WorkspacePermission Permission { get; set; }
    
    public FullWorkspaceViewModel(Guid id, string name, PublicUserViewModel owner, IEnumerable<GroupViewModel> groups, IEnumerable<ExerciseViewModel> exercises, WorkspacePermission permission)
    {
        Id = id;
        Name = name;
        Owner = owner;
        Groups = groups;
        Exercises = exercises;
        Permission = permission;
    }

    public FullWorkspaceViewModel()
    {
        
    }
}