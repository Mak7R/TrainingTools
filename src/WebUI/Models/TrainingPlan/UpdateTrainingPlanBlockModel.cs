using System.ComponentModel.DataAnnotations;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace WebUI.Models.TrainingPlan;

public class UpdateTrainingPlanBlockModel
{
    public List<UpdateTrainingPlanBlockEntryModel> Entries = [];

    [StringLength(MaxBlockNameSize)] public string Title { get; set; } = string.Empty;
}