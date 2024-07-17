using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.Group;

public class CreateGroupModel
{
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(Domain.Rules.DataSizes.GroupDataSizes.MaxNameSize, MinimumLength = Domain.Rules.DataSizes.GroupDataSizes.MinNameSize, ErrorMessage = "Invalid group name length")]
    public string? Name { get; set; }
}