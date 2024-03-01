using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IGroupsService : IAuthorizeService
{
    Task Add(Group group);
    Task<Group?> Get(Expression<Func<Group, bool>> expression);
    Task<IEnumerable<Group>> GetAll();
    Task Update(Guid groupId, UpdateGroupModel groupModel);
    Task Remove(Group group);
    
    public record UpdateGroupModel(string Name);
}