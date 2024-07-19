using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Models;

namespace Application.Services;

public class ExercisesService : IExercisesService
{
    private readonly IExercisesRepository _exercisesRepository;

    public ExercisesService(IExercisesRepository exercisesRepository)
    {
        _exercisesRepository = exercisesRepository;
    }

    public async Task<IEnumerable<Exercise>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var exercises = await _exercisesRepository.GetAll(filterModel);
        
        if (orderModel is null || string.IsNullOrWhiteSpace(orderModel.OrderBy)) return exercises;
        
        if (orderModel.OrderBy.Equals(OrderOptionNames.Exercise.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.OrderOption?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                exercises = exercises.OrderByDescending(e => e.Name);
            }
            else
            {
                exercises = exercises.OrderBy(e => e.Name);
            }
        }
        else if (orderModel.OrderBy.Equals(OrderOptionNames.Exercise.GroupName, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.OrderOption?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                exercises = exercises.OrderByDescending(e => e.Group.Name)
                    .ThenByDescending(e => e.Name);
            }
            else
            {
                exercises = exercises.OrderBy(e => e.Group.Name)
                    .ThenBy(e => e.Name);
            }
        }

        return exercises.ToList();
    }

    public async Task<Exercise?> GetByName(string? name)
    {
        return await _exercisesRepository.GetByName(name);
    }

    public async Task<Exercise?> GetById(Guid id)
    {
        return await _exercisesRepository.GetById(id);
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