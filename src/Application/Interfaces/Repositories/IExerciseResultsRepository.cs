using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IExerciseResultsRepository
{
    Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId);
    Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId, FilterModel? filterModel = null);
    
    Task<OperationResult> Create(ExerciseResult result);
    Task<OperationResult> Update(ExerciseResult result);
    Task<OperationResult> Delete(Guid ownerId, Guid exerciseId);
}