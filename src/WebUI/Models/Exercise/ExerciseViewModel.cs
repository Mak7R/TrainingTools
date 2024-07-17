using WebUI.Models.Group;

namespace WebUI.Models.Exercise;

public class ExerciseViewModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public GroupViewModel Group { get; set; }
    
    public string? About { get; set; }
}