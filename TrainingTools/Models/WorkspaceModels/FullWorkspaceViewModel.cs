using Contracts.Models;

namespace TrainingTools.Models;

public class FullWorkspaceViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public UserViewModel Owner { get; set; }
    
    public IEnumerable<GroupViewModel> Groups { get; set; }
    
    public IEnumerable<ExerciseViewModel> Exercises { get; set; }
    
    public FullWorkspaceViewModel(Workspace workspace, IEnumerable<Group> groups, IEnumerable<Exercise> exercises)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        Owner = new UserViewModel(workspace.Owner);
        Groups = groups.Select(g => new GroupViewModel(g));
        Exercises = exercises.Select(e => new ExerciseViewModel(e));
    }
}