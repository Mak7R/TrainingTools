using Contracts.Models;

namespace TrainingTools.Models;

public class FullWorkspaceViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public UserViewModel Owner { get; set; }
    
    public IEnumerable<GroupViewModel> Groups { get; set; }
    
    public IEnumerable<ExerciseViewModel> Exercises { get; set; }
    
    public FullWorkspaceViewModel(Workspace workspace, IEnumerable<GroupViewModel> groups, IEnumerable<ExerciseViewModel> exercises)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        Owner = new UserViewModel(workspace.Owner);
        Groups = groups;
        Exercises = exercises;
    }
}