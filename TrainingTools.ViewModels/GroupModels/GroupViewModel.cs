﻿using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.ModelContracts;

namespace TrainingTools.ViewModels;

public class GroupViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("workspace")]
    public WorkspaceViewModel Workspace { get; set; }
    
    public GroupViewModel(Guid id, string name, WorkspaceViewModel workspace)
    {
        Id = id;
        Name = name;
        Workspace = workspace;
    }

    public GroupViewModel()
    {
        
    }
}

public class GroupsViewCollectionBuilder : IViewCollectionBuilder<GroupViewModel>
{
    private IEnumerable<GroupViewModel> _collection;
    public GroupsViewCollectionBuilder(IEnumerable<GroupViewModel> exercises)
    {
        _collection = exercises;
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