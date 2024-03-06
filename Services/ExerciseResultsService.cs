using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly TrainingToolsDbContext _dbContext;
    private readonly IAuthorizedUser _authorized;

    public ExerciseResultsService(TrainingToolsDbContext dbContext, IAuthorizedUser authorized)
    {
        _dbContext = dbContext;
        _authorized = authorized;
    }

    public async Task Add(ExerciseResults results)
    {
        var existResults = await _dbContext.ExerciseResults
            .FirstOrDefaultAsync(er => er.OwnerId == _authorized.User.Id && er.ExerciseId == results.ExerciseId);
        
        if (existResults != null) throw new Exception("Results Already Exists");
        results.OwnerId = _authorized.User.Id;
        
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
            .Where(r => r.Owner.Id == _authorized.User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<ExerciseResults>> GetAll()
    {
        return await _dbContext.ExerciseResults
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            .Where(r => r.Owner.Id == _authorized.User.Id)
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
            .Where(r => r.Owner.Id == _authorized.User.Id)
            .FirstOrDefaultAsync(r => r.Id == exerciseResultsId);
        
        if (exerciseResults == null) throw new NotFoundException($"{nameof(ExerciseResults)} with id {exerciseResultsId} was not found");

        updater(exerciseResults);
        
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid exerciseResultsId)
    {
        var exerciseResults = await _dbContext.ExerciseResults
            .Include(r => r.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(r => r.Owner.Id == _authorized.User.Id || r.Exercise.Workspace.Owner.Id == _authorized.User.Id)
            .FirstOrDefaultAsync(r => r.Id == exerciseResultsId);
        
        if (exerciseResults == null) throw new NotFoundException($"{nameof(ExerciseResults)} with id {exerciseResultsId} was not found");
        
        _dbContext.ExerciseResults.Remove(exerciseResults);
    }
}