using WebUI.Models.ExerciseModels;
using WebUI.Models.UserModels;

namespace WebUI.Models.ExerciseResultModels;

public class ExerciseResultViewModel
{
    public UserViewModel Owner { get; set; }
    public ExerciseViewModel Exercise { get; set; }
    
    public IList<ApproachViewModel> ApproachInfos { get; set; }
}