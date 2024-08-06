using Domain.Identity;

namespace Domain.Models.Friendship;

public class FriendInvitation
{
    public ApplicationUser Invitor { get; set; }
    public ApplicationUser Invited { get; set; }

    public DateTime InvitationDateTime { get; set; }
}