using Contracts.Models;
using Services;

namespace TrainingTools.Models;

public class GroupViewModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public WorkspaceViewModel Workspace { get; set; }
    
    public GroupViewModel(Group group)
    {
        Id = group.Id;
        Name = group.Name;
        Workspace = new WorkspaceViewModel(group.Workspace);
    }
}

public class GroupsViewCollectionBuilder : IViewCollectionBuilder<GroupViewModel>
{
    private IEnumerable<GroupViewModel> _collection;
    public GroupsViewCollectionBuilder(IEnumerable<Group> exercises)
    {
        _collection = exercises.Select(g => new GroupViewModel(g));
    }
    public IViewCollectionBuilder<GroupViewModel> Filter(IFilter filter)
    {
        _collection = _collection.Where(filter.FilterBy switch
        {
            nameof(GroupViewModel.Id) => e => e.Id.ToString().Contains(filter.FilterValue!),
            nameof(GroupViewModel.Name) => e => e.Name.Contains(filter.FilterValue!),
            _ => _ => true
        });
        return this;
    }

    public IViewCollectionBuilder<GroupViewModel> Order(IOrder order)
    {
        _collection = (SortBy: order.OrderBy, SortingOption: order.OrderOption) switch
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

    public IEnumerable<GroupViewModel> Build()
    {
        return _collection;
    }
    
    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(GroupViewModel.Id), "Id" },
        { nameof(GroupViewModel.Name), "Name" },
    };
}