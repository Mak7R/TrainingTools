namespace Domain.Models.TrainingPlan;

public class TrainingPlanBlock
{
    public string Title { get; set; }
    public IList<TrainingPlanBlockEntry> TrainingPlanBlockEntries { get; set; }
}