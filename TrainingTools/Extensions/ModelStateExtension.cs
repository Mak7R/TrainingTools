using Microsoft.AspNetCore.Mvc.ModelBinding;
using TrainingTools.ViewModels;

namespace TrainingTools.Extensions;

public static class ModelStateExtension
{
    public static ModelStateErrorViewModel ToModelStateErrorViewModel(this ModelStateDictionary modelState)
    {
        return new ModelStateErrorViewModel(
            new Dictionary<string, IEnumerable<string>?>(
                modelState.Select(i =>
            new KeyValuePair<string, IEnumerable<string>?>(
                i.Key, 
                i.Value?.Errors.Select(e => e.ErrorMessage)))));
    }
}