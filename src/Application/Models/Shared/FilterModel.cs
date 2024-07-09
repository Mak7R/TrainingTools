using System.Linq.Expressions;

namespace Application.Models.Shared;

public class FilterModel : Dictionary<string, string?>
{
    public IQueryable<T> FilterBy<T>(IQueryable<T> queryable, IEnumerable<KeyValuePair<string, Expression<Func<T, bool>>>> filters)
    {
        var result = queryable;
        foreach (var filter in filters)
        {
            result = FilterBy(result, filter.Key, filter.Value);
        }
        return result;
    }

    public IQueryable<T> FilterBy<T>(IQueryable<T> queryable, IEnumerable<KeyValuePair<string, Func<string, Expression<Func<T, bool>>>>> filters)
    {
        var result = queryable;
        foreach (var filter in filters)
        {
            result = FilterBy(result, filter.Key, filter.Value);
        }
        return result;
    }
    
    public IQueryable<T> FilterBy<T>(IQueryable<T> queryable, string key, Expression<Func<T, bool>> predicate)
    {
        if (TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return queryable.Where(predicate);
        }

        return queryable;
    }
    
    public IQueryable<T> FilterBy<T>(IQueryable<T> queryable, string key, Func<string, Expression<Func<T, bool>>> predicateGenerator)
    {
        if (TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return queryable.Where(predicateGenerator(value));
        }

        return queryable;
    }
    
    
    public IEnumerable<T> FilterBy<T>(IEnumerable<T> enumerable, IEnumerable<KeyValuePair<string, Func<T, bool>>> filters)
    {
        var result = enumerable;
        foreach (var filter in filters)
        {
            result = FilterBy(result, filter.Key, filter.Value);
        }
        return result;
    }

    public IEnumerable<T> FilterBy<T>(IEnumerable<T> enumerable, IEnumerable<KeyValuePair<string, Func<string, Func<T, bool>>>> filters)
    {
        var result = enumerable;
        foreach (var filter in filters)
        {
            result = FilterBy(result, filter.Key, filter.Value);
        }
        return result;
    }
    
    public IEnumerable<T> FilterBy<T>(IEnumerable<T> enumerable, string key, Func<T, bool> predicate)
    {
        if (TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return enumerable.Where(predicate);
        }

        return enumerable;
    }
    
    public IEnumerable<T> FilterBy<T>(IEnumerable<T> enumerable, string key, Func<string, Func<T, bool>> predicateGenerator)
    {
        if (TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return enumerable.Where(predicateGenerator(value));
        }

        return enumerable;
    }
}