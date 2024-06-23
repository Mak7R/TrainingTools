using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IExerciseResultsRepository
{
    Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId);
    Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId, FilterModel? filterModel = null);
    
    Task<OperationResult> CreateResult(ExerciseResult result);
    Task<OperationResult> UpdateResult(ExerciseResult result);
    Task<OperationResult> DeleteResult(Guid ownerId, Guid exerciseId);
}