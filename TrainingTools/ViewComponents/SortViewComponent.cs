using Microsoft.AspNetCore.Mvc;
using TrainingTools.Models;

namespace TrainingTools.ViewComponents;

public class SortViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        SortingComponentView model)
    {
        string? icon = null;
        string? nextOption = null;
        if (model.Value == ViewBag.SortBy)
        {
            using (var enumerator = model.Options.GetEnumerator())
            {
                for ( ;enumerator.MoveNext(); )
                {
                    if (enumerator.Current.Key != ViewBag.SortingOption) continue;
                
                    icon = enumerator.Current.Value;

                    nextOption = enumerator.MoveNext() ? 
                        enumerator.Current.Key : 
                        model.Options.First(_ => true).Key;
                    break;
                }
            }
            if (icon == null || nextOption == null) throw new Exception("Option was not found");
        }

        icon ??= model.Options.First(_ => true).Value;
        int i = 0;
        nextOption ??= model.Options.First(_ => i++ == 1).Key;
        
        return View(new SortingViewModel
        {
            SortBy = model.Value,
            Display = model.Display,
            Option = nextOption,
            IconPartialView = icon,
        });
    }
}