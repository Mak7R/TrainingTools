using Application.Constants;

namespace WebUI.Extensions;

public static class QueryHelperExtensions
{
    public static string AsFilterName(this string value) => $"{FilterOptionNames.Shared.FiltersPrefix}{value}";
}