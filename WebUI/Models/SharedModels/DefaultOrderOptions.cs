namespace WebUI.Models.SharedModels;

public class DefaultOrderOptions : IOrderOptions
{
    private static readonly List<string> Options =
    [
        "asc",
        "desc",
        ""
    ];

    private int _current;
    
    public IOrderOptions Set(string? current)
    {
        if (string.IsNullOrEmpty(current))
        {
            _current = 0;
        }

        for (var i = 0; i < Options.Count; i++)
        {
            if (Options[i] != current) continue;
            _current = i;
            return this;
        }

        _current = 0;
        return this;
    }

    public IOrderOptions MoveNext()
    {
        if (++_current == Options.Count)
        {
            _current = 0;
        }

        return this;
    }

    public string Current => Options[_current];
}