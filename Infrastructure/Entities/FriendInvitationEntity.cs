using System.ComponentModel.DataAnnotations.Schema;
using Application.Identity;

namespace Infrastructure.Entities;

public class FriendInvitationEntity
{
    [ForeignKey(nameof(Invitor))]
    public Guid InvitorId { get; set; }
    public ApplicationUser Invitor { get; set; }
    
    [ForeignKey(nameof(Target))]
    public Guid TargetId { get; set; }
    public ApplicationUser Target { get; set; }
    
    public DateTime InvitationTime { get; set; }
}