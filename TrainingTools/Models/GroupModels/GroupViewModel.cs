using Contracts.Models;

namespace TrainingTools.Models;

public class GroupViewModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public WorkspaceViewModel Workspace { get; set; }
    
    public GroupViewModel(Group group)
    {
        Id = group.Id;
        Name = group.Name;
        Workspace = new WorkspaceViewModel(group.Workspace);
    }
}