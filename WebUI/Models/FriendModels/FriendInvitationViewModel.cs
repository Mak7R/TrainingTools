using WebUI.Models.UserModels;

namespace WebUI.Models.FriendModels;

public class FriendInvitationViewModel
{
    public UserViewModel Invitor { get; set; }
    public UserViewModel Target { get; set; }
    public DateTime InvitationTime { get; set; }
}