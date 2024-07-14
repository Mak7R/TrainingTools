using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace WebUI.Models.TrainingPlanModels;

public class UpdateTrainingPlanModel
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(MaxTitleSize, MinimumLength = MinTitleSize, ErrorMessage = "Title length is invalid")]
    public string NewTitle { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; }

    public List<UpdateTrainingPlanBlockModel> Blocks { get; set; } = [];
}