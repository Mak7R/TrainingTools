using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;

namespace Contracts.Models;

public class FollowerRelationship
{
    [ForeignKey(nameof(Workspace))]
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
    
    [ForeignKey(nameof(Follower))]
    public Guid FollowerId { get; set; }
    public User Follower { get; set; }
    
    public FollowerRights FollowerRights { get; set; }
}