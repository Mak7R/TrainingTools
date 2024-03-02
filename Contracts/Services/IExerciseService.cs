using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IExercisesService : IAuthorizeService
{
    Task Add(Exercise exercise);
    Task<Exercise?> Get(Expression<Func<Exercise, bool>> expression);
    Task<IEnumerable<Exercise>> GetAll();
    Task Update(Guid exerciseId, Action<Exercise> updater);
    Task Remove(Exercise exercise);
}