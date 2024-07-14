using Domain.Identity;

namespace Domain.Models.TrainingPlan;

public class TrainingPlan
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ApplicationUser Author { get; set; }
    public bool IsPublic { get; set; }
    public List<TrainingPlanBlock> TrainingPlanBlocks { get; set; }
}