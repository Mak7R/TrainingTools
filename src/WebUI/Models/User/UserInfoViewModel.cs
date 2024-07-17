using Application.Enums;

namespace WebUI.Models.User;

public class UserInfoViewModel
{
    public UserViewModel User { get; set; }
    public RelationshipState RelationshipState { get; set; }
    public IEnumerable<string> Roles { get; set; }
}