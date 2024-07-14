namespace Application.Models.Shared;

public class PageModel
{
    public int CurrentPage { get; set; }

    public IEnumerable<T> SelectPage<T>(IEnumerable<T> enumerable, int pageSize)
    {
        return enumerable.Skip(CurrentPage * pageSize).Take(pageSize);
    }
    
    public IQueryable<T> SelectPage<T>(IQueryable<T> enumerable, int pageSize)
    {
        return enumerable.Skip(CurrentPage * pageSize).Take(pageSize);
    }
}