using Contracts.Models;

namespace TrainingTools.Models;

public class FullExerciseViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public WorkspaceViewModel Workspace { get; set; }
    public GroupViewModel? Group { get; set; }
    public ExerciseResultsViewModel? Results { get; set; }
    
    public FullExerciseViewModel(Exercise exercise, ExerciseResults? results)
    {
        Id = exercise.Id;
        Name = exercise.Name;
        Workspace = new WorkspaceViewModel(exercise.Workspace);
        Group = exercise.Group == null ? null : new GroupViewModel(exercise.Group);
        Results = results == null ? null : new ExerciseResultsViewModel(results);
    }
}