using Domain.Models;

namespace WebUI.Models.ExerciseResultModels;

public class UpdateResultsModel
{
    public IList<ApproachViewModel> ApproachInfos { get; set; }
}