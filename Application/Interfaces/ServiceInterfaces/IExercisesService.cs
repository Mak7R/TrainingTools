using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExercisesService
{
    Task<OperationResult> CreateExercise(Exercise? exercise);
    
    Task<IEnumerable<Exercise>> GetAll();
    Task<Exercise?> GetByName(string? name);
    Task<Exercise?> GetById(Guid id);
    
    Task<OperationResult> UpdateExercise(Exercise? group);
    Task<OperationResult> DeleteExercise(Guid id);
}