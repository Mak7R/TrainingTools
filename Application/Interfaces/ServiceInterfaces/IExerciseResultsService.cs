using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExerciseResultsService
{
    Task<OperationResult> CreateResult(ExerciseResult result);
    Task<OperationResult> UpdateResult(ExerciseResult result);
    Task<OperationResult> DeleteResult(Guid ownerId, Guid exerciseId);

    Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId);
    Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    
    
}