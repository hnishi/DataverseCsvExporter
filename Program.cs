using DataverseCsvExporter.Models;
using DataverseCsvExporter.Services;
using Microsoft.Extensions.Logging;

namespace DataverseCsvExporter;

public class Program
{
    private static ILoggerFactory CreateLoggerFactory(Configuration config)
    {
        return LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(config.Logging.GetLogLevel());
        });
    }

    public static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            var configManager = new ConfigurationManager();
            configManager.LoadConfiguration();
            var config = configManager.GetSettings();

            // Initialize logger factory
            using var loggerFactory = CreateLoggerFactory(config);

            // Initialize Dataverse client and connect
            var dataverseLogger = loggerFactory.CreateLogger<DataverseClient>();
            var client = new DataverseClient(config, dataverseLogger);
            await client.Connect();

            // Initialize CSV exporter
            var exporterLogger = loggerFactory.CreateLogger<CsvExporter>();
            var exporter = new CsvExporter(config, exporterLogger);

            // Retrieve and export data
            var maxItemsMessage = config.Export.MaxItemCount.HasValue
                ? $"(max {config.Export.MaxItemCount.Value:N0} records)"
                : "(no limit)";

            ErrorHandler.LogToConsole($"Starting export: Entity={config.Export.Entity}, View={config.Export.View} {maxItemsMessage}");

            var data = client.RetrieveData(
                config.Export.Entity,
                config.Export.View,
                config.Export.PageSize,
                config.Export.MaxItemCount);

            await exporter.ExportData(data);

            ErrorHandler.LogToConsole("Export completed successfully.");
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleError(ex);
            Environment.Exit(1);
        }
    }
}
