namespace WebUI.Models.TrainingPlan;

public class TrainingPlanBlockViewModel
{
    public string Title { get; set; } = string.Empty;
    public List<TrainingPlanBlockEntryViewModel> TrainingPlanBlockEntries { get; set; }
}