using System.Text.Json.Serialization;
using Contracts.ModelContracts;

namespace TrainingTools.ViewModels;

public class ExerciseViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("workspace")]
    public WorkspaceViewModel Workspace { get; set; }
    [JsonPropertyName("group")]
    public GroupViewModel? Group { get; set; }
    public ExerciseViewModel(Guid id, string name, WorkspaceViewModel workspace, GroupViewModel? group)
    {
        Id = id;
        Name = name;
        Workspace = workspace;
        Group = group;
    }

    public ExerciseViewModel()
    {
        
    }
}

public class ExercisesViewCollectionBuilder : IViewCollectionBuilder<ExerciseViewModel>
{
    private IEnumerable<ExerciseViewModel> _collection;
    public ExercisesViewCollectionBuilder(IEnumerable<ExerciseViewModel> exercises)
    {
        _collection = exercises;
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