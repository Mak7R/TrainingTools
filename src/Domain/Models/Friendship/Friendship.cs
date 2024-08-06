using Domain.Identity;

namespace Domain.Models.Friendship;

public class Friendship
{
    public ApplicationUser FirstFriend { get; set; }
    public ApplicationUser SecondFriend { get; set; }

    public DateTime FriendsFromDateTime { get; set; }
}