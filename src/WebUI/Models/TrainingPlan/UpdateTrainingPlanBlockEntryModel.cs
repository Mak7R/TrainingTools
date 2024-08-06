using System.ComponentModel.DataAnnotations;
using Domain.Rules;

namespace WebUI.Models.TrainingPlan;

public class UpdateTrainingPlanBlockEntryModel
{
    [StringLength(DataSizes.TrainingPlanDataSizes.MaxBlockEntryDescriptionSize,
        ErrorMessage = "Description size cannot be more than {0}")]
    public string? Description;

    public Guid GroupId { get; set; }
}