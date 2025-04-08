using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DataverseCsvExporter.Models;

public class Configuration
{
    public Configuration()
    {
        Dataverse = new DataverseConfig();
        Export = new ExportConfig();
        Logging = new LoggingConfig();
    }

    [JsonPropertyName("dataverse")]
    public DataverseConfig Dataverse { get; set; }

    [JsonPropertyName("export")]
    public ExportConfig Export { get; set; }

    [JsonPropertyName("logging")]
    public LoggingConfig Logging { get; set; }

    public void Validate()
    {
        Dataverse?.Validate();
        Export?.Validate();
        Logging?.Validate();
    }
}

public class DataverseConfig
{
    public DataverseConfig()
    {
    }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    public void Validate()
    {
        if (string.IsNullOrEmpty(Url))
        {
            throw new ArgumentException("Dataverse URL must be specified.");
        }

        if (string.IsNullOrEmpty(Username))
        {
            throw new ArgumentException("Username must be specified.");
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new ArgumentException("Password must be specified.");
        }
    }
}

public class ExportConfig
{
    public ExportConfig()
    {
        Output = new OutputConfig();
        PageSize = 5000;
    }

    [JsonPropertyName("entity")]
    public string Entity { get; set; } = null!;

    [JsonPropertyName("view")]
    public string View { get; set; } = null!;

    [JsonPropertyName("output")]
    public OutputConfig Output { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("maxItemCount")]
    public int? MaxItemCount { get; set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Entity))
        {
            throw new ArgumentException("Entity name must be specified.");
        }

        if (string.IsNullOrEmpty(View))
        {
            throw new ArgumentException("View name must be specified.");
        }

        if (PageSize <= 0)
        {
            throw new ArgumentException("Page size must be greater than 0.");
        }

        if (MaxItemCount.HasValue && MaxItemCount.Value <= 0)
        {
            throw new ArgumentException("Maximum item count must be greater than 0 if specified.");
        }

        Output?.Validate();
    }
}

public class OutputConfig
{
    public OutputConfig()
    {
    }

    [JsonPropertyName("directory")]
    public string Directory { get; set; } = null!;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = null!;

    [JsonPropertyName("useBom")]
    public bool UseBom { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrEmpty(Directory))
        {
            throw new ArgumentException("Output directory must be specified.");
        }

        if (string.IsNullOrEmpty(FileName))
        {
            throw new ArgumentException("Output file name must be specified.");
        }
    }
}

public class LoggingConfig
{
    public LoggingConfig()
    {
        MinimumLevel = "Information";
    }

    [JsonPropertyName("minimumLevel")]
    public string MinimumLevel { get; set; }

    public LogLevel GetLogLevel()
    {
        return MinimumLevel?.ToLower() switch
        {
            "trace" => LogLevel.Trace,
            "debug" => LogLevel.Debug,
            "information" => LogLevel.Information,
            "warning" => LogLevel.Warning,
            "error" => LogLevel.Error,
            "critical" => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }

    public void Validate()
    {
        var validLevels = new[] { "trace", "debug", "information", "warning", "error", "critical" };
        if (!validLevels.Contains(MinimumLevel?.ToLower()))
        {
            throw new ArgumentException(
                $"Invalid log level: {MinimumLevel}. Valid values are: {string.Join(", ", validLevels)}");
        }
    }
}
