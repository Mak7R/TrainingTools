using Contracts.Models;

namespace TrainingTools.Models;

public class ExerciseViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public WorkspaceViewModel Workspace { get; set; }
    public GroupViewModel? Group { get; set; }
    public ExerciseViewModel(Exercise exercise)
    {
        Id = exercise.Id;
        Name = exercise.Name;
        Workspace = new WorkspaceViewModel(exercise.Workspace);
        Group = exercise.Group == null ? null : new GroupViewModel(exercise.Group);
    }
}