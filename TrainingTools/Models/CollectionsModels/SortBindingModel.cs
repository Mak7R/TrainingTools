using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models;

public class SortBindingModel
{
    public string? SortBy { get; set; }
    public string? SortingOption { get; set; }
    
    [BindNever]
    public bool HasSorters => !string.IsNullOrEmpty(SortBy) && !string.IsNullOrEmpty(SortingOption);
}