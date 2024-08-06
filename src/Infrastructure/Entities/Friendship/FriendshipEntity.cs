using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity;

namespace Infrastructure.Entities.Friendship;

public class FriendshipEntity
{
    [ForeignKey(nameof(FirstFriend))] public Guid FirstFriendId { get; set; }

    public ApplicationUser FirstFriend { get; set; }

    [ForeignKey(nameof(SecondFriend))] public Guid SecondFriendId { get; set; }

    public ApplicationUser SecondFriend { get; set; }

    public DateTime FriendsFrom { get; set; }
}