using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace WebUI.Models.TrainingPlan;

public class CreateTrainingPlanModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(MaxTitleSize, MinimumLength = MinTitleSize, ErrorMessage = "Title length is invalid")]
    [Remote("IsTitleFree", "TrainingPlans", ErrorMessage = "Training plan with this name already exists")]
    public string Title { get; set; }
    
    public bool IsPublic { get; set; }
}