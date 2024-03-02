using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IExerciseResultsService : IAuthorizeService
{
    Task Add(ExerciseResults results);
    Task<ExerciseResults?> Get(Expression<Func<ExerciseResults, bool>> expression);
    Task<IEnumerable<ExerciseResults>> GetAll();
    Task Update(Guid exerciseResultsId, Action<ExerciseResults> updater);
    Task Remove(ExerciseResults exerciseResults);
}