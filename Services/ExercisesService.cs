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

    public async Task AddAsync(Exercise exercise)
    {
        var workspace = await _dbContext.Workspaces.FirstOrDefaultAsync(w => w.Id == exercise.WorkspaceId);
        if (workspace == null) throw new NotFoundException("Workspace was not found");
        exercise.Workspace = workspace;
        
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Exercise?> GetExerciseAsync(Expression<Func<Exercise, bool>> expression)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Exercise>> GetExercisesAsync()
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .ToListAsync();
    }

    public async Task UpdateAsync(Guid exerciseId, IExercisesService.UpdateExerciseModel exerciseModel)
    {
        var exercise = await _dbContext.Exercises
            .Include(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(e => e.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        if (exercise == null) throw new NotFoundException($"{nameof(Exercise)} with id {exerciseId} was not found");
        
        exercise.Name = exerciseModel.Name;

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(Exercise exercise)
    {
        if (exercise.Workspace.Owner.Id != User.Id) throw new OperationNotAllowedException("User is not owner of this exercise");
        _dbContext.Exercises.Remove(exercise);
        await _dbContext.SaveChangesAsync();
    }
}