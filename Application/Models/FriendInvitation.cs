using Application.Identity;

namespace Application.Models;

public class FriendInvitation
{
    public ApplicationUser Invitor { get; set; }
    public ApplicationUser Target { get; set; }
    public DateTime InvitationTime { get; set; }
}