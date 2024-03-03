using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Models;

public class Group
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    
    [ForeignKey(nameof(Workspace))]
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
    
    public List<Exercise> Exercises { get; set; }
}