namespace Domain.Models.TrainingPlan;

public class TrainingPlanBlock
{
    public string Name { get; set; }
    public List<TrainingPlanBlockEntry> TrainingPlanBlockEntries { get; set; }
}