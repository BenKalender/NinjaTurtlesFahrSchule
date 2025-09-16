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
        // Try to parse the string to a DateTime.
        if (DateTime.TryParseExact(dateString, _supportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            // Set the Kind to UTC to resolve the PostgreSQL error.
            return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }
        // Return null if parsing fails.
        return null;
    }

    private static readonly string[] _supportedFormats = new[]
    {
        // ISO 8601 (uluslararası standart)
        "yyyy-MM-dd",
        // Genellikle Türk kullanıcıların kullandığı format
        "dd.MM.yyyy",
        // Slash'li yaygın formatlar (ABD'de ve diğer ülkelerde yaygın)
        "MM/dd/yyyy",
        "dd/MM/yyyy",

        // Tireli yaygın formatlar
        "yyyy-MM-ddTHH:mm:ss", // Tam tarih ve saat
        "yyyy-MM-ddTHH:mm:ssZ" // Zaman dilimi bilgisiyle

    };
}


