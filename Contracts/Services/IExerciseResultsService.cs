using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IExerciseResultsService
{
    Task Add(ExerciseResults results);
    Task<ExerciseResults?> Get(Expression<Func<ExerciseResults, bool>> expression);
    Task<IEnumerable<ExerciseResults>> GetRange(Expression<Func<ExerciseResults, bool>> expression);
    Task Update(Guid exerciseResultsId, Action<ExerciseResults> updater);
    Task Remove(Guid exerciseResultsId);
}