using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models;

public class FilterModel
{
    public string? SearchBy { get; set; }
    public string? SearchValue { get; set; }

    [BindNever]
    public bool HasFilters => !string.IsNullOrEmpty(SearchBy) && !string.IsNullOrEmpty(SearchValue);
    
    [BindNever]
    public Dictionary<string, string> SearchByOptions { get; set; }
}