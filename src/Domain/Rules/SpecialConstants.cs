namespace Domain.Rules;

public static class SpecialConstants
{
    public const char DefaultSeparator = '\0';

    public static bool IsStringValid(string str) => !str.Contains(DefaultSeparator);
}