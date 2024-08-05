using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Models;

namespace Application.Services;

public class ExercisesService : IExercisesService
{
    private readonly IRepository<Exercise, Guid> _exercisesRepository;

    public ExercisesService(IRepository<Exercise, Guid> exercisesRepository)
    {
        _exercisesRepository = exercisesRepository;
    }

    public async Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        return await _exercisesRepository.GetAll(filterModel, orderModel, pageModel);
    }

    public async Task<Exercise?> GetByName(string? name)
    {
        return (await _exercisesRepository.GetAll(new FilterModel{{FilterOptionNames.Exercise.NameEquals, name}}, null, new PageModel{PageSize = 1})).SingleOrDefault();
    }

    public async Task<Exercise?> GetById(Guid id)
    {
        return await _exercisesRepository.GetById(id);
    }
    
    public async Task<int> Count(FilterModel? filterModel = null)
    {
        return await _exercisesRepository.Count(filterModel);
    }
    
    public async Task<OperationResult> Create(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        exercise.Id = Guid.NewGuid();
        exercise.Name = exercise.Name?.Trim();
        return await _exercisesRepository.Create(exercise);
    }

    public async Task<OperationResult> Update(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(exercise.Group);
        
        exercise.Name = exercise.Name?.Trim();
        return await _exercisesRepository.Update(exercise);
    }

    public async Task<OperationResult> Delete(Guid id)
    {
        return await _exercisesRepository.Delete(id);
    }
}