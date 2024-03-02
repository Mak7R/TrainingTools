using System.Linq.Expressions;
using Contracts;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class ExercisesService : IExercisesService
{
    private readonly TrainingToolsDbContext _dbContext;
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public ExercisesService(TrainingToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public async Task Add(Exercise exercise)
    {
        var workspace = await _dbContext.Workspaces
            .Where(w => w.Owner.Id == User.Id)
            .FirstOrDefaultAsync(w => w.Id == exercise.WorkspaceId);
        
        if (workspace == null) throw new NotFoundException("Workspace was not found");
        exercise.Workspace = workspace;
        
        if (exercise.GroupId.HasValue)
        {
            var group = await _dbContext.Groups
                .Include(g => g.Workspace)
                .FirstOrDefaultAsync(g => g.Id == exercise.GroupId.Value);

            if (group != null && group.WorkspaceId != exercise.WorkspaceId)
                throw new OperationNotAllowedException("Group and exercise exists in different workspaces");
                
            exercise.Group = group;
        }
        else
        {
            exercise.Group = null;
        }
        
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Exercise?> Get(Expression<Func<Exercise, bool>> expression)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Exercise>> GetAll()
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .ToListAsync();
    }

    public async Task Update(Guid exerciseId, Action<Exercise> updater)
    {
        var exercise = await _dbContext.Exercises
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");

        updater(exercise);
        
        // I can paste here all checks and security. Like check user changed or another errors.
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Exercise exercise)
    {
        if (exercise.Workspace.Owner.Id != User.Id) throw new OperationNotAllowedException("User is not owner of this exercise");
        _dbContext.Exercises.Remove(exercise);
        await _dbContext.SaveChangesAsync();
    }
}