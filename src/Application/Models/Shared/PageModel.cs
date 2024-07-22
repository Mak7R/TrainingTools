using Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Shared;

public class PageModel
{
    public const int DefaultPageSize = int.MaxValue;
    public virtual int CurrentPage { get; set; } = 0;
    public virtual int PageSize { get; set; } = DefaultPageSize;

    public IEnumerable<T> TakePage<T>(IEnumerable<T> enumerable)
    {
        var result = enumerable;
        
        if (CurrentPage > 0 && PageSize > 0)
            result = result.Skip(CurrentPage * PageSize);
        if (PageSize > 0)
            result = result.Take(PageSize);

        return result;
    }
    
    public IQueryable<T> TakePage<T>(IQueryable<T> queryable)
    {
        var result = queryable;
        
        if (CurrentPage > 0 && PageSize > 0)
            result = result.Skip(CurrentPage * PageSize);
        if (PageSize > 0)
            result = result.Take(PageSize);

        return result;
    }
}