using WebUI.Models.User;

namespace WebUI.Models.Friend;

public class FriendInvitationViewModel
{
    public UserViewModel Invitor { get; set; }
    public UserViewModel Target { get; set; }
    public DateTime InvitationTime { get; set; }
}