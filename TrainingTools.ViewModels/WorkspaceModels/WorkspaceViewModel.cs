using System.Text.Json.Serialization;
using Contracts.ModelContracts;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class WorkspaceViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("owner")]
    public UserViewModel Owner { get; set; }
    
    public WorkspaceViewModel(Workspace workspace)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        Owner = new UserViewModel(workspace.Owner);
    }

    public WorkspaceViewModel()
    {
        
    }
}

public class WorkspacesViewCollectionBuilder : IViewCollectionBuilder<WorkspaceViewModel>
{
    private IEnumerable<WorkspaceViewModel> _collection;

    public WorkspacesViewCollectionBuilder(IEnumerable<Workspace> workspaces)
    {
        _collection = workspaces.Select(w => new WorkspaceViewModel(w));
    }
    
    public IViewCollectionBuilder<WorkspaceViewModel> Filter(IFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.FilterBy) && !string.IsNullOrEmpty(filter.FilterValue))
        {
            _collection = _collection.Where(filter.FilterBy switch
            {
                nameof(WorkspaceViewModel.Id) => w => w.Id.ToString().Contains(filter.FilterValue!),
                nameof(WorkspaceViewModel.Name) => w => w.Name.Contains(filter.FilterValue!),
                _ => _ => true
            });
        }
        
        return this;
    }

    public IViewCollectionBuilder<WorkspaceViewModel> Order(IOrder order)
    {
        if (!string.IsNullOrEmpty(order.OrderBy) && !string.IsNullOrEmpty(order.OrderOption))
        {
            _collection = (OrderBy: order.OrderBy, OrderOption: order.OrderOption) switch
            {
                (nameof(WorkspaceViewModel.Name), "A-Z") => 
                    _collection.OrderBy(w => w.Name),
                (nameof(WorkspaceViewModel.Name), "Z-A") => 
                    _collection.OrderBy(w => w.Name).Reverse(),
                (nameof(WorkspaceViewModel.Id), "ASCENDING") => 
                    _collection.OrderBy(w => w.Id),
                (nameof(WorkspaceViewModel.Id), "DESCENDING") => 
                    _collection.OrderBy(w => w.Id).Reverse(),
                _ => _collection
            };
        }
        
        return this;
    }

    public IEnumerable<WorkspaceViewModel> Build()
    {
        return _collection;
    }

    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(WorkspaceViewModel.Id), "Id" },
        { nameof(WorkspaceViewModel.Name), "Name" }
    };
}