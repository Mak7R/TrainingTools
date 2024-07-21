using WebUI.Models.User;

namespace WebUI.Models.Friend;

public class FriendInvitationViewModel
{
    public UserViewModel Invitor { get; set; }
    public UserViewModel Invited { get; set; }
    public DateTime InvitationDateTime { get; set; }
}