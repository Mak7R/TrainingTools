namespace TrainingTools.ViewModels;

public class FollowViewModel
{
    public Guid WorkspaceId { get; set; }
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