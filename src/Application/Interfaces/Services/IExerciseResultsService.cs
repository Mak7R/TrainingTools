﻿using Application.Models.Shared;
using Domain.Identity;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IExerciseResultsService
{
    Task<ExerciseResult?> GetById(Guid ownerId, Guid exerciseId);
    Task<int> Count(FilterModel? filterModel);

    Task<IEnumerable<ExerciseResult>> GetForUser(string ownerUserName, FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null);

    Task<IEnumerable<ExerciseResult>> GetForExercise(string groupName, string exerciseName,
        FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null);

    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(ApplicationUser user, string groupName,
        string exerciseName, FilterModel? filterModel = null, OrderModel? orderModel = null,
        PageModel? pageModel = null);
    
    Task<OperationResult> Create(ExerciseResult result);
    Task<OperationResult> Update(ExerciseResult result);
    Task<OperationResult> Delete(Guid ownerId, Guid exerciseId);
}