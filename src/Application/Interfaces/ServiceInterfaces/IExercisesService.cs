﻿using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExercisesService
{
    Task<IEnumerable<Exercise>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<Exercise?> GetByName(string? name);
    Task<Exercise?> GetById(Guid id);
    
    Task<OperationResult> CreateExercise(Exercise? exercise);
    Task<OperationResult> UpdateExercise(Exercise? group);
    Task<OperationResult> DeleteExercise(Guid id);
}