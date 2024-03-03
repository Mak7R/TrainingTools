using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class UsersAuthorizer : IUsersAuthorizer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;

    public UsersAuthorizer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dbContext = _serviceProvider.GetRequiredService<TrainingToolsDbContext>();
    }
    
    public T GetServiceForUser<T>(User user) where T: IAuthorizeService
    {
        var service = _serviceProvider.GetRequiredService<T>();
        service.SetUser(user);
        return service;
    }

    public async Task Add(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public async Task<User?> Get(Expression<Func<User, bool>> expression)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(expression);
    }
    
    public async Task<IEnumerable<User>> GetAll()
    {
        return await _dbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task Update(Guid userId, Action<User> updater)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new NotFoundException($"{nameof(User)} with id {userId} was not found");

        updater(user);
        
        // I can paste here all checks and security. Like check user changed or another errors.
        
    }

    public async Task Remove(Guid userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Workspaces)
            .Include(u => u.UserResults)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null) throw new NotFoundException($"{nameof(User)} with id {userId} was not found");

        var workspacesService = GetServiceForUser<IWorkspacesService>(user);
        var exerciseResultsService = GetServiceForUser<IExerciseResultsService>(user);

        foreach (var workspace in user.Workspaces) await workspacesService.Remove(workspace.Id);
        foreach (var userResult in user.UserResults) await exerciseResultsService.Remove(userResult.Id);
        
        _dbContext.Users.Remove(user);
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }
}