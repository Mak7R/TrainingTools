using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IUsersAuthorizer
{
    T GetServiceForUser<T>(User user) where T: IAuthorizeService;
    Task Add(User user);
    Task<User?> Get(Expression<Func<User, bool>> expression);
    Task<IEnumerable<User>> GetAll();
    Task Update(Guid userId, Action<User> updater);
    Task Remove(Guid userId);

    Task SaveChanges();
}