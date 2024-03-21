using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Extensions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly TrainingToolsDbContext _dbContext;
    private readonly ISelectedWorkspace _selected;

    public ExerciseResultsService(TrainingToolsDbContext dbContext, ISelectedWorkspace selected)
    {
        _dbContext = dbContext;
        _selected = selected;
    }

    public async Task Add(ExerciseResults results)
    {
        if (!_selected.Permission.HasUsePermission()) throw new HasNotPermissionException();
        
        var existResults = await _dbContext.ExerciseResults
            .FirstOrDefaultAsync(er => er.ExerciseId == results.ExerciseId && er.OwnerId == _selected.Authorized.User.Id);
        
        if (existResults != null) throw new AlreadyExistsException("User results already exists for this exercise");
        
        results.Id = Guid.NewGuid();
        results.OwnerId = _selected.Authorized.User.Id;
        
        await _dbContext.ExerciseResults.AddAsync(results);
    }


    public async Task<ExerciseResults?> Get(Expression<Func<ExerciseResults, bool>> expression)
    {
        return await _dbContext.ExerciseResults
            .AsNoTracking()
            
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            
            .Where(r => r.Owner.Equals(_selected.Authorized.User))
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<ExerciseResults>> GetRange(Expression<Func<ExerciseResults, bool>> expression)
    {
        return await _dbContext.ExerciseResults
            .AsNoTracking()
            
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            
            .Where(r => r.Owner.Equals(_selected.Authorized.User) || 
                        (r.Exercise.WorkspaceId == _selected.Workspace.Id && _selected.Permission.HasViewPermission()))
            .Where(expression)
            .ToListAsync();
    }

    public async Task Update(Guid exerciseResultsId, Action<ExerciseResults> updater)
    {
        var exerciseResults = await _dbContext.ExerciseResults
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            
            .Where(r => r.Owner.Equals(_selected.Authorized.User))
            .FirstOrDefaultAsync(r => r.Id == exerciseResultsId);
        
        if (exerciseResults == null) throw new NotFoundException($"{nameof(ExerciseResults)} with id {exerciseResultsId} was not found");

        updater(exerciseResults);

        if (!exerciseResults.Owner.Equals(_selected.Authorized.User)) throw new HasNotPermissionException("Impossible to change owner of exercise result");
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid exerciseResultsId)
    {
        var exerciseResults = await _dbContext.ExerciseResults
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(r => r.Owner.Equals(_selected.Authorized.User) ||
                        (r.Exercise.WorkspaceId == _selected.Workspace.Id && _selected.Permission.HasOwnerPermission()))
            .FirstOrDefaultAsync(r => r.Id == exerciseResultsId);
        
        if (exerciseResults == null) throw new NotFoundException($"{nameof(ExerciseResults)} with id {exerciseResultsId} was not found");
        
        _dbContext.ExerciseResults.Remove(exerciseResults);
    }
}