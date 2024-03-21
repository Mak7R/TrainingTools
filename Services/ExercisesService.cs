using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Extensions;
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
    private readonly ISelectedWorkspace _selected;

    public ExercisesService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, ISelectedWorkspace selected)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _selected = selected;
    }

    public async Task Add(Exercise exercise)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        exercise.Id = Guid.NewGuid();
        exercise.WorkspaceId = _selected.Workspace.Id;
        
        if (exercise.GroupId.HasValue)
        {
            var group = await _dbContext.Groups
                .Where(g => g.WorkspaceId == _selected.Workspace.Id)
                .FirstOrDefaultAsync(g => g.Id == exercise.GroupId.Value);

            if (group == null) throw new NotFoundException("Group was not found");
            
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
        if (!_selected.Permission.HasViewPermission()) throw new HasNotPermissionException();
        
        return await _dbContext.Exercises
            .AsNoTracking()
            
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(e => e.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Exercise>> GetAll()
    {
        if (!_selected.Permission.HasViewPermission()) throw new HasNotPermissionException();
        
        return await _dbContext.Exercises
            .AsNoTracking()
            
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(e => e.WorkspaceId == _selected.Workspace.Id)
            .ToListAsync();
    }

    public async Task Update(Guid exerciseId, Action<Exercise> updater)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        var exercise = await _dbContext.Exercises
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Group)
            .ThenInclude(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(e => e.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");
        
        updater(exercise);
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid exerciseId)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        var exercise = await _dbContext.Exercises
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(e => e.Results)
            
            .Where(e => e.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");

        var resultsService = _serviceProvider.GetRequiredService<IExerciseResultsService>();
        
        foreach (var result in exercise.Results) await resultsService.Remove(result.Id);
        
        _dbContext.Exercises.Remove(exercise);
    }
}