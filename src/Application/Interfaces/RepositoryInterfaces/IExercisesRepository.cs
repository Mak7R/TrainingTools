﻿using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IExercisesRepository
{
    Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null);
    Task<Exercise?> GetByName(string? name);
    Task<Exercise?> GetById(Guid id);
    
    Task<OperationResult> CreateExercise(Exercise? exercise);
    Task<OperationResult> UpdateExercise(Exercise? exercise);
    Task<OperationResult> DeleteExercise(Guid id);
}