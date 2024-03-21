using System.Text.Json.Serialization;
using Contracts.ModelContracts;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class PublicUserViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    public PublicUserViewModel(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }
    
    public PublicUserViewModel()
    {
        
    }
}

public class PublicUserViewCollectionBuilder : IViewCollectionBuilder<PublicUserViewModel>
{
    private IEnumerable<PublicUserViewModel> _collection;

    public PublicUserViewCollectionBuilder(IEnumerable<PublicUserViewModel> users)
    {
        _collection = users;
    }
    
    public IViewCollectionBuilder<PublicUserViewModel> Filter(IFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.FilterBy) && !string.IsNullOrEmpty(filter.FilterValue))
        {
            _collection = _collection.Where(filter.FilterBy switch
            {
                nameof(PublicUserViewModel.Id) => w => w.Id.ToString().Contains(filter.FilterValue!),
                nameof(PublicUserViewModel.Name) => w => w.Name.Contains(filter.FilterValue!),
                nameof(PublicUserViewModel.Email) => w => w.Email.Contains(filter.FilterValue!),
                _ => _ => true
            });
        }
        
        return this;
    }

    public IViewCollectionBuilder<PublicUserViewModel> Order(IOrder order)
    {
        if (!string.IsNullOrEmpty(order.OrderBy) && !string.IsNullOrEmpty(order.OrderOption))
        {
            _collection = (OrderBy: order.OrderBy, OrderOption: order.OrderOption) switch
            {
                (nameof(PublicUserViewModel.Name), "A-Z") => 
                    _collection.OrderBy(w => w.Name),
                (nameof(PublicUserViewModel.Name), "Z-A") => 
                    _collection.OrderBy(w => w.Name).Reverse(),
                (nameof(PublicUserViewModel.Email), "A-Z") => 
                    _collection.OrderBy(w => w.Email),
                (nameof(PublicUserViewModel.Email), "Z-A") => 
                    _collection.OrderBy(w => w.Email).Reverse(),
                (nameof(PublicUserViewModel.Id), "ASCENDING") => 
                    _collection.OrderBy(w => w.Id),
                (nameof(PublicUserViewModel.Id), "DESCENDING") => 
                    _collection.OrderBy(w => w.Id).Reverse(),
                _ => _collection
            };
        }
        
        return this;
    }

    public IEnumerable<PublicUserViewModel> Build()
    {
        return _collection;
    }

    public static readonly Dictionary<string, string> FilterByOptions = new()
    {
        { nameof(PublicUserViewModel.Id), "Id" },
        { nameof(PublicUserViewModel.Name), "Name" },
        { nameof(PublicUserViewModel.Email), "Email"}
    };
}