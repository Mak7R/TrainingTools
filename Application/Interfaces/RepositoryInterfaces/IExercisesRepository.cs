using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IExercisesRepository
{
    Task<OperationResult> CreateGroup(Exercise? exercise);
    
    Task<IEnumerable<Exercise>> GetAll();
    Task<Group?> GetByName(string? name);
    Task<Group?> GetById(Guid id);
    
    Task<OperationResult> UpdateGroup(Exercise? exercise);
    Task<OperationResult> DeleteGroup(Guid id);
}