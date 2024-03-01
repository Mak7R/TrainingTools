using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IExercisesService : IAuthorizeService
{
    Task Add(Exercise exercise);
    Task<Exercise?> Get(Expression<Func<Exercise, bool>> expression);
    Task<IEnumerable<Exercise>> GetAll();
    Task Update(Guid exerciseId, UpdateExerciseModel exerciseModel);
    Task Remove(Exercise exercise);

    public record UpdateExerciseModel(string Name, Guid? GroupId);
}