using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Models;

public class Exercise
{
    public const int MaxNameLength = 32;
    
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(MaxNameLength)]
    public string Name { get; set; }
    
    
    [ForeignKey(nameof(Workspace))]
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
    
    
    [ForeignKey(nameof(Group))]
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    
    public List<ExerciseResults> Results { get; set; }
}