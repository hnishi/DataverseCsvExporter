using System;

namespace DataverseCsvExporter.Services;

public interface ILogFormatter
{
    string FormatMessage(string message);
}

public class TimestampLogFormatter : ILogFormatter
{
    private readonly string _dateFormat;

    public TimestampLogFormatter(string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        _dateFormat = dateFormat;
    }

    public string FormatMessage(string message)
    {
        var timestamp = DateTime.Now.ToString(_dateFormat);
        return $"[{timestamp}] {message}";
    }
}
