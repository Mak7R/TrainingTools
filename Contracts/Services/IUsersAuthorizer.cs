using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IUsersAuthorizer
{
    T GetServiceForUser<T>(User user) where T: IAuthorizeService;
    Task Add(User user);
    Task<User?> Get(Expression<Func<User, bool>> expression);
    Task<IEnumerable<User>> GetAll();
    Task Update(Guid userId, UpdateUserModel userModel);
    Task Remove(User user);
    
    public record UpdateUserModel(string Name, string Email, string Password);
}