using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.TrainingPlan;

public class UpdateTrainingPlanBlockEntryModel
{
    public Guid GroupId { get; set; }
    
    [StringLength(Domain.Rules.DataSizes.TrainingPlanDataSizes.MaxBlockEntryDescriptionSize, ErrorMessage = "Description size cannot be more than {0}")]
    public string? Description;
}