using DataverseCsvExporter.Models;

namespace DataverseCsvExporter.Services;

public class DateFormatter
{
    private readonly DateFormatConfig _config;
    private static readonly TimeSpan JstOffset = TimeSpan.FromHours(9);

    public DateFormatter(DateFormatConfig config)
    {
        _config = config;
    }

    public string FormatDateTime(DateTime utcDateTime, bool isDateOnly)
    {
        if (_config.EnableJstConversion)
        {
            utcDateTime = utcDateTime.Add(JstOffset);
        }

        return utcDateTime.ToString(
            isDateOnly ? _config.DateFormat : _config.DateTimeFormat
        );
    }
}
