using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class UsersService : IUsersService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthorizedUser _authorized;
    private readonly TrainingToolsDbContext _dbContext;
    
    public UsersService(IServiceProvider serviceProvider, IAuthorizedUser authorized, TrainingToolsDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _authorized = authorized;
        _dbContext = dbContext;
    }

    public async Task Add(User user)
    {
        var existUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existUser != null) throw new AlreadyExistsException("User with this email already exists");
        
        user.Id = Guid.NewGuid();
        
        await _dbContext.Users.AddAsync(user);
    }

    public async Task<User> Get(Expression<Func<User, bool>> expression)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            
            .FirstOrDefaultAsync(expression);

        if (user == null) throw new NotFoundException("User was not found");
        
        if (_authorized.IsAuthorized && user.Equals(_authorized.User))
        {
            // loading another private data
            await _dbContext.Users.Entry(user).Collection(u => u.UserResults).LoadAsync();
            var follows = _dbContext.Users.Entry(user).Collection(u => u.Follows);
            await follows.LoadAsync();

            if (follows.CurrentValue != null)
            {
                foreach (var follow in follows.CurrentValue)
                    await _dbContext.FollowerRelationships.Entry(follow).Reference(f => f.Workspace).LoadAsync();
            }
        }
        else
        {
            // hiding another private data
            user.Password = string.Empty;
        }

        return user;
    }
    
    public async Task<IEnumerable<User>> GetAll()
    {
        var users = _dbContext.Users.AsNoTracking();
        await users.ForEachAsync(u => u.Password = string.Empty);
        return await users.ToListAsync();
    }

    public async Task Update(Guid userId, Action<User> updater)
    {
        if (!(_authorized.User.Id == userId /*or is server admin*/)) throw new HasNotPermissionException();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new NotFoundException($"{nameof(User)} with id {userId} was not found");

        updater(user);
        
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid userId)
    {
        if (!(_authorized.User.Id == userId /*or is server admin*/)) throw new HasNotPermissionException();
        
        var user = await _dbContext.Users
            .Include(u => u.Workspaces)
            .Include(u => u.UserResults)
            .Include(user => user.Follows)
            
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null) throw new NotFoundException($"{nameof(User)} with id {userId} was not found");
        
        var workspacesService = _serviceProvider.GetRequiredService<IWorkspacesService>();
        var exerciseResultsService = _serviceProvider.GetRequiredService<IExerciseResultsService>();

        foreach (var workspace in user.Workspaces) await workspacesService.Remove(workspace.Id);
        foreach (var userResult in user.UserResults) await exerciseResultsService.Remove(userResult.Id);
        foreach (var follow in user.Follows) _dbContext.FollowerRelationships.Remove(follow);

        _dbContext.Users.Remove(user);
    }
}