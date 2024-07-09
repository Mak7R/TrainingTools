using WebUI.Models.GroupModels;

namespace WebUI.Models.TrainingPlanModels;

public class TrainingPlanBlockEntryViewModel
{
    public GroupViewModel Group { get; set; }
    public string Description { get; set; }
}