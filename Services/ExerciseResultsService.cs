﻿using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly TrainingToolsDbContext _dbContext;
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public ExerciseResultsService(TrainingToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public async Task Add(ExerciseResults results)
    {
        var existResults = await _dbContext.ExerciseResults
            .FirstOrDefaultAsync(er => er.OwnerId == User.Id && er.ExerciseId == results.ExerciseId);
        
        if (existResults != null) throw new Exception("Results Already Exists");
        results.OwnerId = User.Id;
        
        await _dbContext.ExerciseResults.AddAsync(results);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ExerciseResults?> Get(Expression<Func<ExerciseResults, bool>> expression)
    {
        return await _dbContext.ExerciseResults
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Results)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            .Where(r => r.Owner.Id == User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<ExerciseResults>> GetAll()
    {
        return await _dbContext.ExerciseResults
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Results)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            .Where(r => r.Owner.Id == User.Id)
            .ToListAsync();
    }

    public async Task Update(Guid exerciseResultsId, Action<ExerciseResults> updater)
    {
        var exerciseResults = await _dbContext.ExerciseResults
            .Include(r => r.Owner)
            .Include(r => r.Results)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(r => r.Exercise)
            .ThenInclude(e => e.Group)
            .Where(r => r.Owner.Id == User.Id)
            .FirstOrDefaultAsync(r => r.Id == exerciseResultsId);
        
        if (exerciseResults == null) throw new NotFoundException($"{nameof(ExerciseResults)} with id {exerciseResultsId} was not found");

        updater(exerciseResults);
        foreach (var result in exerciseResults.Results)
        {
            // first get all from db and then make check ???
            if (await _dbContext.ExerciseResults.FirstOrDefaultAsync(r => r.Id == result.Id) == null)
            {
                await _dbContext.ExerciseResultEntries.AddAsync(result);
            }
        }
        // I can paste here all checks and security. Like check user changed or another errors.
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(ExerciseResults exerciseResults)
    {
        if (exerciseResults.Owner.Id != User.Id) throw new OperationNotAllowedException("User is not owner of this exercise results");
        _dbContext.ExerciseResults.Remove(exerciseResults);
        await _dbContext.SaveChangesAsync();
    }
}