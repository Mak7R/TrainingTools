using Domain.Identity;

namespace Domain.Models;



public class ExerciseResult
{
    public ApplicationUser Owner { get; set; }
    public Exercise Exercise { get; set; }
    
    public IList<Approach> ApproachInfos { get; set; }
}