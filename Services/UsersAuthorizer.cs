using System.Linq.Expressions;
using Contracts;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class UsersAuthorizer : IDisposable, IUsersAuthorizer
{
    private readonly IServiceScope _scope;
    private readonly TrainingToolsDbContext _dbContext;

    public UsersAuthorizer(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TrainingToolsDbContext>();
    }
    
    public T GetServiceForUser<T>(User user) where T: IAuthorizeService
    {
        var service = _scope.ServiceProvider.GetRequiredService<T>();
        service.SetUser(user);
        return service;
    }

    public async Task Add(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> Get(Expression<Func<User, bool>> expression)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(expression);
    }
    
    public async Task<IEnumerable<User>> GetAll()
    {
        return await _dbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task Update(Guid userId, IUsersAuthorizer.UpdateUserModel userModel)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new NotFoundException($"{nameof(User)} with id {userId} was not found");

        user.Email = userModel.Email;
        user.Password = userModel.Password;
        user.Name = userModel.Name;

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(User user)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }
    
    public void Dispose() // danger | virtual Dispose(true)
    {
        _scope.Dispose();
    }
}