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
    private readonly IAuthorizedUser _authorizedUser;
    private readonly TrainingToolsDbContext _dbContext;
    
    public UsersService(IServiceProvider serviceProvider, IAuthorizedUser authorizedUser, TrainingToolsDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _authorizedUser = authorizedUser;
        _dbContext = dbContext;
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

        if (user.Id == _authorizedUser.User.Id /*or _authorizedUser.User.IsAdmin*/)
        {
            var workspacesService = _serviceProvider.GetRequiredService<IWorkspacesService>();
            var exerciseResultsService = _serviceProvider.GetRequiredService<IWorkspacesService>();

            foreach (var workspace in user.Workspaces) await workspacesService.Remove(workspace.Id);
            foreach (var userResult in user.UserResults) await exerciseResultsService.Remove(userResult.Id);

            _dbContext.Users.Remove(user);
        }
        else
        {
            throw new OperationNotAllowedException();
        }
    }
}