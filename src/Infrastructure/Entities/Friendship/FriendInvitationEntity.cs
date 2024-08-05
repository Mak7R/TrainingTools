using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity;

namespace Infrastructure.Entities.Friendship;

public class FriendInvitationEntity
{
    [ForeignKey(nameof(Invitor))]
    public Guid InvitorId { get; set; }
    public ApplicationUser Invitor { get; set; }
    
    [ForeignKey(nameof(Invited))]
    public Guid InvitedId { get; set; }
    public ApplicationUser Invited { get; set; }
    
    public DateTime InvitationDateTime { get; set; }
}