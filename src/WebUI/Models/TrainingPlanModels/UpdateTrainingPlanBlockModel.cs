using System.ComponentModel.DataAnnotations;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace WebUI.Models.TrainingPlanModels;

public class UpdateTrainingPlanBlockModel
{
    [StringLength(MaxBlockNameSize)]
    public string Name { get; set; } = string.Empty;

    public List<UpdateTrainingPlanBlockEntryModel> Entries = [];
}