using Microsoft.Extensions.Configuration;
using DataverseCsvExporter.Models;
using System.Text.Json;

namespace DataverseCsvExporter.Services;

public class ConfigurationManager
{
    private readonly IConfiguration _configuration;
    private Configuration? _settings;

    public ConfigurationManager(string configPath = "config.json")
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configPath, optional: false)
            .Build();
    }

    public void LoadConfiguration()
    {
        var jsonConfig = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
        _settings = JsonSerializer.Deserialize<Configuration>(jsonConfig, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        ValidateConfiguration();
    }

    public void ValidateConfiguration()
    {
        if (_settings == null)
            throw new InvalidOperationException("Configuration has not been loaded.");

        if (string.IsNullOrEmpty(_settings.Dataverse.Url))
            throw new ArgumentException("Dataverse URL is required.");

        if (string.IsNullOrEmpty(_settings.Dataverse.Username))
            throw new ArgumentException("Dataverse username is required.");

        if (string.IsNullOrEmpty(_settings.Dataverse.Password))
            throw new ArgumentException("Dataverse password is required.");

        if (string.IsNullOrEmpty(_settings.Export.Entity))
            throw new ArgumentException("Export entity name is required.");

        if (string.IsNullOrEmpty(_settings.Export.View))
            throw new ArgumentException("Export view name is required.");

        if (_settings.Export.PageSize <= 0)
            throw new ArgumentException("Export page size must be greater than 0.");
    }

    public Configuration GetSettings()
    {
        if (_settings == null)
            throw new InvalidOperationException("Configuration has not been loaded.");

        return _settings;
    }
}
