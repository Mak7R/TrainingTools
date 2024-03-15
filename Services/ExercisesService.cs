using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class ExercisesService : IExercisesService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;
    private readonly IAuthorizedUser _authorized;

    public ExercisesService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, IAuthorizedUser authorized)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _authorized = authorized;
    }

    public async Task Add(Exercise exercise)
    {
        exercise.Id = Guid.NewGuid();
        
        var workspace = await _dbContext.Workspaces
            .Where(w => w.Owner.Equals(_authorized.User)) // this is a check of rights to add exercise to workspace
            .FirstOrDefaultAsync(w => w.Id == exercise.WorkspaceId);
        
        if (workspace == null) throw new NotFoundException("Workspace was not found");
        
        exercise.Workspace = workspace;
        
        if (exercise.GroupId.HasValue)
        {
            var group = await _dbContext.Groups
                .Include(g => g.Workspace)
                .Where(g => g.Workspace.Equals(workspace)) // this is a check of rights to use group for exercise (Is a group in same workspace with exercise)
                .FirstOrDefaultAsync(g => g.Id == exercise.GroupId.Value);
                
            exercise.Group = group;
        }
        else
        {
            exercise.Group = null;
        }
        
        await _dbContext.Exercises.AddAsync(exercise);
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
            
            .Where(e => e.Workspace.Owner.Equals(_authorized.User))
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
            
            .Where(e => e.Workspace.Owner.Equals(_authorized.User))
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
            
            .Where(e => e.Workspace.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");
        
        updater(exercise);
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid exerciseId)
    {
        var exercise = await _dbContext.Exercises
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Results)
            
            .Where(e=> e.Workspace.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");

        var resultsService = _serviceProvider.GetRequiredService<IExerciseResultsService>();
        
        foreach (var result in exercise.Results) await resultsService.Remove(result.Id);
        
        _dbContext.Exercises.Remove(exercise);
    }
}