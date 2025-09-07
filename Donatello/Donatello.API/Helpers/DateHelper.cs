using System;
using System.Globalization;

namespace Donatello.API.Helpers;

public static class DateHelper
{
    /// <summary>
    /// Given a string in "DD.MM.YYYY" format, this method parses it to a UTC DateTime.
    /// </summary>
    /// <param name="dateString">The date string to parse.</param>
    /// <returns>A UTC-based DateTime object, or null if parsing fails.</returns>
    public static DateTime? ParseToUtcDateTime(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        // Define the culture and format to be used for parsing.
        var culture = new CultureInfo("tr-TR");
        var formats = new[] { "dd.MM.yyyy", "yyyy-MM-dd" };

        // Try to parse the string to a DateTime.
        if (DateTime.TryParseExact(dateString, formats, culture, DateTimeStyles.None, out DateTime parsedDate))
        {
            // Set the Kind to UTC to resolve the PostgreSQL error.
            return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        // Return null if parsing fails.
        return null;
    }
}
