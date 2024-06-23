using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.GroupModels;

public class AddGroupModel
{
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(Domain.Rules.DataSizes.Group.MaxNameSize, MinimumLength = Domain.Rules.DataSizes.Group.MinNameSize, ErrorMessage = "Invalid group name length")]
    public string? Name { get; set; }
}