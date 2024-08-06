using WebUI.Models.User;

namespace WebUI.Models.TrainingPlan;

public class TrainingPlanViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public UserViewModel Author { get; set; }
    public bool IsPublic { get; set; }

    public List<TrainingPlanBlockViewModel> TrainingPlanBlocks { get; set; }
}