using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class AuthorizedUser : IAuthorizedUser
{
    private User? _user = null;
    private readonly TrainingToolsDbContext _dbContext;
    private readonly ICookiesSession _session;

    public User User => _user ?? throw new Exception("User was not authorized");
    public bool IsAuthorized => _user != null;
    public bool IsPasswordConfirmed { get; private set; } = false;

    public AuthorizedUser(TrainingToolsDbContext dbContext, ICookiesSession session)
    {
        _dbContext = dbContext;
        _session = session;
    }

    private async Task Authorize(Expression<Func<User, bool>> expression)
    {
        _user = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Workspaces)
            .Include(u => u.UserResults)
            .FirstOrDefaultAsync(expression);

        if (_user == null) throw new NotFoundException("User was not found");
    }

    public async Task<bool> Authorize(HttpContext context)
    {
        if (!_session.GetAuthentication(context, out Guid userId))
            return false;
        await Authorize(u => u.Id == userId);
        return true;
    }

    public async Task<bool> Authorize(HttpContext context, string email, string password)
    {
        await Authorize(u => u.Email == email);

        if (!ConfirmPassword(password))
        {
            _user = null;
            return false;
        }
        _session.AddAuthentication(context, User.Id);
        return true;
    }

    public void EndAuthorization(HttpContext context)
    {
        _session.RemoveAuthentication(context);
        _user = null;
        IsPasswordConfirmed = false;
    }

    public bool ConfirmPassword(string password)
    {
        if (User.Password != password) return false;
        
        IsPasswordConfirmed = true;
        return true;
    }

    public async Task SaveChanges()
    {
        // possible writing of changes to get who do what, when and other
        await _dbContext.SaveChangesAsync();
    }
}