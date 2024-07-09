using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IExercisesRepository
{
    Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null);
    Task<Exercise?> GetByName(string? name);
    Task<Exercise?> GetById(Guid id);
    
    Task<OperationResult> Create(Exercise? exercise);
    Task<OperationResult> Update(Exercise? exercise);
    Task<OperationResult> Delete(Guid id);
}