
using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.ModelContracts;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class FollowerViewModel
{
    [JsonPropertyName("follower")]
    public PublicUserViewModel Follower { get; set; }
    
    [JsonPropertyName("rights")]
    public FollowerRights Rights { get; set; }

    public FollowerViewModel(PublicUserViewModel follower, FollowerRights rights)
    {
        Follower = follower;
        Rights = rights;
    }

    public FollowerViewModel()
    {
        
    }
}

public class FollowersViewCollectionBuilder : IViewCollectionBuilder<FollowerViewModel>
{
    private IEnumerable<FollowerViewModel> _collection;

    public FollowersViewCollectionBuilder(IEnumerable<FollowerViewModel> followers)
    {
        _collection = followers;
    }
    
    public IViewCollectionBuilder<FollowerViewModel> Filter(IFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.FilterBy) && !string.IsNullOrEmpty(filter.FilterValue))
        {
            _collection = _collection.Where(filter.FilterBy switch
            {
                nameof(FollowerViewModel.Follower) => fr => fr.Follower.Name.ToString().Contains(filter.FilterValue!),
                nameof(FollowerViewModel.Rights) => fr => fr.Rights.ToString().Contains(filter.FilterValue!),
                _ => _ => true
            });
        }
        
        return this;
    }

    public IViewCollectionBuilder<FollowerViewModel> Order(IOrder order)
    {
        if (!string.IsNullOrEmpty(order.OrderBy) && !string.IsNullOrEmpty(order.OrderOption))
        {
            _collection = (OrderBy: order.OrderBy, OrderOption: order.OrderOption) switch
            {
                (nameof(FollowerViewModel.Follower), "A-Z") => 
                    _collection.OrderBy(fr => fr.Follower.Name),
                (nameof(FollowerViewModel.Follower), "Z-A") => 
                    _collection.OrderBy(fr => fr.Follower.Name).Reverse(),
                (nameof(FollowerViewModel.Rights), "ASCENDING") => 
                    _collection.OrderBy(fr => fr.Rights),
                (nameof(FollowerViewModel.Rights), "DESCENDING") => 
                    _collection.OrderBy(fr => fr.Rights).Reverse(),
                _ => _collection
            };
        }
        
        return this;
    }

    public IEnumerable<FollowerViewModel> Build()
    {
        return _collection;
    }

    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(FollowerViewModel.Follower), "Follower" },
        { nameof(FollowerViewModel.Rights), "Rights" }
    };
}