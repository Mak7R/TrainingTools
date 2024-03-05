using Contracts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Server.HttpSys;

namespace TrainingTools.Models;

public class FilterModel : IFilter
{
    private string _filterBy = string.Empty;
    public string FilterBy
    {
        get => _filterBy;
        set
        {
            if (value == null) return;
            else _filterBy = value;
        }
    }

    private string _filterValue = string.Empty;
    public string FilterValue
    {
        get => _filterValue;
        set
        {
            if (value == null) return;
            else _filterValue = value;
        }
    }
    
    [BindNever]
    public Dictionary<string, string> FilterByOptions { get; set; }
}