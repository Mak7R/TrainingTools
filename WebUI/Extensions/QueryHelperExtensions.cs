namespace WebUI.Extensions;

public static class QueryHelperExtensions
{
    public static string AsFilterName(this string value) => $"f_{value}";
}