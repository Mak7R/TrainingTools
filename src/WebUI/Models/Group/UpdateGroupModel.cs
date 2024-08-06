using System.ComponentModel.DataAnnotations;
using Domain.Rules;

namespace WebUI.Models.Group;

public class UpdateGroupModel
{
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(DataSizes.GroupDataSizes.MaxNameSize, MinimumLength = DataSizes.GroupDataSizes.MinNameSize,
        ErrorMessage = "Invalid group name length")]
    public string? Name { get; set; }
}