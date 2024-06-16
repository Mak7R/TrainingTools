using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Models;

namespace Application.Services;

public class ExercisesService : IExercisesService
{
    private readonly IExercisesRepository _exercisesRepository;

    public ExercisesService(IExercisesRepository exercisesRepository)
    {
        _exercisesRepository = exercisesRepository;
    }
    
    public async Task<OperationResult> CreateExercise(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        exercise.Id = Guid.NewGuid();
        exercise.Name = exercise.Name?.Trim();
        return await _exercisesRepository.CreateExercise(exercise);
    }

    public async Task<IEnumerable<Exercise>> GetAll()
    {
        return await _exercisesRepository.GetAll();
    }

    public async Task<Exercise?> GetByName(string? name)
    {
        return await _exercisesRepository.GetByName(name);
    }

    public async Task<Exercise?> GetById(Guid id)
    {
        return await _exercisesRepository.GetById(id);
    }

    public async Task<OperationResult> UpdateExercise(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(exercise.Group);
        
        exercise.Name = exercise.Name?.Trim();
        return await _exercisesRepository.UpdateExercise(exercise);
    }

    public async Task<OperationResult> DeleteExercise(Guid id)
    {
        return await _exercisesRepository.DeleteExercise(id);
    }
}