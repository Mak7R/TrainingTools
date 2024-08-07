﻿using System.Globalization;

namespace WebUI.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string str)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(str.ToLower());
    }
}