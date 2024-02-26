using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IExercisesService : IAuthorizeService
{
    Task AddAsync(Exercise exercise);
    Task<Exercise?> GetExerciseAsync(Expression<Func<Exercise, bool>> expression);
    Task<IEnumerable<Exercise>> GetExercisesAsync();
    Task UpdateAsync(Guid exerciseId, UpdateExerciseModel exerciseModel);
    Task RemoveAsync(Exercise exercise);

    public record UpdateExerciseModel(string Name);
}