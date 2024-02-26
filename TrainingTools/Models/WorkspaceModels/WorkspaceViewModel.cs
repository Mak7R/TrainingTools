
using Contracts.Models;

namespace TrainingTools.Models;

public class WorkspaceViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public UserViewModel Owner { get; set; }
    
    public WorkspaceViewModel(Workspace workspace)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        Owner = new UserViewModel(workspace.Owner);
    }
}