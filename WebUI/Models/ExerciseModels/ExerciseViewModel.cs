using WebUI.Models.GroupModels;

namespace WebUI.Models.ExerciseModels;

public class ExerciseViewModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public GroupViewModel Group { get; set; }
}