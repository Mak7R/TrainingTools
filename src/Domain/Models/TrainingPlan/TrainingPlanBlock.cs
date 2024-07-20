namespace Domain.Models.TrainingPlan;

public class TrainingPlanBlock
{
    public string Title { get; set; }
    public List<TrainingPlanBlockEntry> TrainingPlanBlockEntries { get; set; }
}