using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Models;

public class Workspace
{
    public const int MaxNameLength = 32;
    
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(MaxNameLength)]
    public string Name { get; set; }
    
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }

    public bool IsPublic { get; set; } = false;
    
    public List<Group> Groups { get; set; }
    public List<Exercise> Exercises { get; set; }
    
    public List<FollowerRelationship> Followers { get; set; }

    public override bool Equals(object? obj) => obj is Workspace workspace && Id.Equals(workspace.Id);

    public override int GetHashCode() => Id.GetHashCode();
}