namespace Domain.Models.TrainingPlan;

public class TrainingPlanBlock
{
    public string Title { get; set; } = string.Empty;
    public IList<TrainingPlanBlockEntry> TrainingPlanBlockEntries { get; set; }
}