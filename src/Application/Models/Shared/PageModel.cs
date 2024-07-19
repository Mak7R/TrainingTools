using Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Shared;

public class PageModel
{
    public int CurrentPage { get; set; } = 0;
    public int PageSize { get; set; } = int.MaxValue;
    public int PagesCount { get; set; } = 1;

    public IEnumerable<T> TakePage<T>(IEnumerable<T> enumerable)
    {
        var result = enumerable;
        
        if (CurrentPage > 0 && PageSize > 0)
            result = result.Skip(CurrentPage * PageSize);
        if (PageSize > 0)
            result = result.Take(PageSize * PagesCount);

        return result;
    }
    
    public IQueryable<T> TakePage<T>(IQueryable<T> queryable)
    {
        var result = queryable;
        
        if (CurrentPage > 0 && PageSize > 0)
            result = result.Skip(CurrentPage * PageSize);
        if (PageSize > 0)
            result = result.Take(PageSize * PagesCount);

        return result;
    }
}