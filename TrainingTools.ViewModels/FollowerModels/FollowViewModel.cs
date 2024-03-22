using System.Text.Json.Serialization;

namespace TrainingTools.ViewModels;

public class FollowViewModel
{
    [JsonPropertyName("workspaceid")]
    public Guid WorkspaceId { get; set; }
    
    [JsonPropertyName("workspacename")]
    public string WorkspaceName { get; set; }
    
    public FollowViewModel(Guid workspaceId, string workspaceName)
    {
        WorkspaceId = workspaceId;
        WorkspaceName = workspaceName;
    }
    
    public FollowViewModel()
    {
        
    }
}