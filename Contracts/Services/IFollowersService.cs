using Contracts.Models;

namespace Contracts.Services;

public interface IFollowersService
{
    Task<IEnumerable<FollowerRelationship>> GetFollows();
    Task<IEnumerable<FollowerRelationship>> GetFollowers();
    Task AddFollower(Guid workspaceId);
    Task UpdateFollower(Guid followerId, Action<FollowerRelationship> updater);
    Task RemoveFollower(Guid followerId);
}