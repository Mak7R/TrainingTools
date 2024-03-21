using Contracts.Enums;

namespace Contracts.Extensions;

public static class FollowerRightsExtension
{
    public static bool HasViewRights(this FollowerRights followerRights)
    {
        return followerRights switch
        {
            FollowerRights.PendingAccess => false,
            FollowerRights.ViewOnly => true,
            FollowerRights.UseOnly => true,
            FollowerRights.All => true,
            _ => throw new ArgumentOutOfRangeException(nameof(followerRights), followerRights, null)
        };
    }
    
    public static bool HasUseRights(this FollowerRights followerRights)
    {
        return followerRights switch
        {
            FollowerRights.PendingAccess => false,
            FollowerRights.ViewOnly => false,
            FollowerRights.UseOnly => true,
            FollowerRights.All => true,
            _ => throw new ArgumentOutOfRangeException(nameof(followerRights), followerRights, null)
        };
    }
    
    public static bool HasAllRights(this FollowerRights followerRights)
    {
        return followerRights switch
        {
            FollowerRights.PendingAccess => false,
            FollowerRights.ViewOnly => false,
            FollowerRights.UseOnly => false,
            FollowerRights.All => true,
            _ => throw new ArgumentOutOfRangeException(nameof(followerRights), followerRights, null)
        };
    }
}