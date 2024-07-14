using Domain.Models.TrainingPlan;
using WebUI.Models.UserModels;

namespace WebUI.Models.TrainingPlanModels;

public class TrainingPlanViewModel
{
    public string Title { get; set; }
    public UserViewModel Author { get; set; }
    public bool IsPublic { get; set; }
    
    public List<TrainingPlanBlockViewModel> TrainingPlanBlocks { get; set; }
}