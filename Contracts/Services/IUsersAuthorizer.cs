using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IUsersAuthorizer
{
    T GetServiceForUser<T>(User user) where T: IAuthorizeService;
    Task AddAsync(User user);
    Task<User?> GetUserAsync(Expression<Func<User, bool>> expression);
    Task<IEnumerable<User>> GetUsersAsync();
    Task UpdateUserAsync(Guid userId, UpdateUserModel userModel);
    Task RemoveAsync(User user);
    
    public record UpdateUserModel(string Name, string Email, string Password);
}