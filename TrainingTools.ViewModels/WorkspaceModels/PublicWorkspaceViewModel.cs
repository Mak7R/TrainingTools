using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.ModelContracts;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class PublicWorkspaceViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("owner")]
    public PublicUserViewModel Owner { get; set; }

    [JsonPropertyName("permission")]
    public WorkspacePermission Permission { get; set; }
    
    public PublicWorkspaceViewModel(Guid id, string name, PublicUserViewModel owner, WorkspacePermission permission)
    {
        Id = id;
        Name = name;
        Owner = owner;
        Permission = permission;
    }

    public PublicWorkspaceViewModel()
    {
        
    }
}

public class PublicWorkspacesViewCollectionBuilder : IViewCollectionBuilder<PublicWorkspaceViewModel>
{
    private IEnumerable<PublicWorkspaceViewModel> _collection;

    public PublicWorkspacesViewCollectionBuilder(IEnumerable<PublicWorkspaceViewModel> workspaces)
    {
        _collection = workspaces;
    }
    
    public IViewCollectionBuilder<PublicWorkspaceViewModel> Filter(IFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.FilterBy) && !string.IsNullOrEmpty(filter.FilterValue))
        {
            _collection = _collection.Where(filter.FilterBy switch
            {
                nameof(PublicWorkspaceViewModel.Id) => w => w.Id.ToString().Contains(filter.FilterValue!),
                nameof(PublicWorkspaceViewModel.Name) => w => w.Name.Contains(filter.FilterValue!),
                nameof(PublicWorkspaceViewModel.Owner) => w => w.Owner.Name.Contains(filter.FilterValue!),
                _ => _ => true
            });
        }
        
        return this;
    }

    public IViewCollectionBuilder<PublicWorkspaceViewModel> Order(IOrder order)
    {
        if (!string.IsNullOrEmpty(order.OrderBy) && !string.IsNullOrEmpty(order.OrderOption))
        {
            _collection = (OrderBy: order.OrderBy, OrderOption: order.OrderOption) switch
            {
                (nameof(PublicWorkspaceViewModel.Name), "A-Z") => 
                    _collection.OrderBy(w => w.Name),
                (nameof(PublicWorkspaceViewModel.Name), "Z-A") => 
                    _collection.OrderBy(w => w.Name).Reverse(),
                (nameof(PublicWorkspaceViewModel.Id), "ASCENDING") => 
                    _collection.OrderBy(w => w.Id),
                (nameof(PublicWorkspaceViewModel.Id), "DESCENDING") => 
                    _collection.OrderBy(w => w.Id).Reverse(),
                (nameof(PublicWorkspaceViewModel.Owner), "A-Z") => 
                    _collection.OrderBy(w => w.Owner.Name),
                (nameof(PublicWorkspaceViewModel.Owner), "Z-A") => 
                    _collection.OrderBy(w => w.Owner.Name).Reverse(),
                _ => _collection
            };
        }
        
        return this;
    }

    public IEnumerable<PublicWorkspaceViewModel> Build()
    {
        return _collection;
    }

    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(PublicWorkspaceViewModel.Id), "Id" },
        { nameof(PublicWorkspaceViewModel.Name), "Name" },
        { nameof(PublicWorkspaceViewModel.Owner), "Owner" }
    };
}