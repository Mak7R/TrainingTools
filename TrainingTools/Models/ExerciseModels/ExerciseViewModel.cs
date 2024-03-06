using Contracts.ModelContracts;
using Contracts.Models;
using Services;

namespace TrainingTools.Models;

public class ExerciseViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public WorkspaceViewModel Workspace { get; set; }
    public GroupViewModel? Group { get; set; }
    public ExerciseViewModel(Exercise exercise)
    {
        Id = exercise.Id;
        Name = exercise.Name;
        Workspace = new WorkspaceViewModel(exercise.Workspace);
        Group = exercise.Group == null ? null : new GroupViewModel(exercise.Group);
    }
}

public class ExercisesViewCollectionBuilder : IViewCollectionBuilder<ExerciseViewModel>
{
    private IEnumerable<ExerciseViewModel> _collection;
    public ExercisesViewCollectionBuilder(IEnumerable<Exercise> exercises)
    {
        _collection = exercises.Select(e => new ExerciseViewModel(e));
    }
    public IViewCollectionBuilder<ExerciseViewModel> Filter(IFilter filter)
    {
        _collection = _collection.Where(filter.FilterBy switch
        {
            nameof(ExerciseViewModel.Id) => e => e.Id.ToString().Contains(filter.FilterValue!),
            nameof(ExerciseViewModel.Name) => e => e.Name.Contains(filter.FilterValue!),
            _ => _ => true
        });
        return this;
    }

    public IViewCollectionBuilder<ExerciseViewModel> Order(IOrder order)
    {
        _collection = (OrderBy: order.OrderBy, OrderOption: order.OrderOption) switch
        {
            (nameof(ExerciseViewModel.Name), "A-Z") => 
                _collection.OrderBy(w => w.Name),
            (nameof(ExerciseViewModel.Name), "Z-A") => 
                _collection.OrderBy(w => w.Name).Reverse(),
            (nameof(ExerciseViewModel.Id), "ASCENDING") => 
                _collection.OrderBy(w => w.Id),
            (nameof(ExerciseViewModel.Id), "DESCENDING") => 
                _collection.OrderBy(w => w.Id).Reverse(),
            _ => _collection
        };
        return this;
    }

    public IEnumerable<ExerciseViewModel> Build()
    {
        return _collection;
    }
    
    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(ExerciseViewModel.Id), "Id" },
        { nameof(ExerciseViewModel.Name), "Name" },
    };
}