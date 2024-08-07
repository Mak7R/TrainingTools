using WebUI.Models.Exercise;
using WebUI.Models.User;

namespace WebUI.Models.ExerciseResult;

public class ExerciseResultViewModel
{
    public UserViewModel Owner { get; set; }
    public ExerciseViewModel Exercise { get; set; }

    public IList<ApproachViewModel> ApproachInfos { get; set; }
}