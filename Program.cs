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
        ILoggerFactory? loggerFactory = null;
        LoggingService? loggingService = null;

        try
        {
            // Load configuration
            var configManager = new ConfigurationManager();
            configManager.LoadConfiguration();
            var config = configManager.GetSettings();

            // Initialize logger factory
            loggerFactory = CreateLoggerFactory(config);

            // Initialize logging components
            var logger = loggerFactory.CreateLogger("DataverseCsvExporter");
            var logFormatter = new TimestampLogFormatter();
            loggingService = new LoggingService(logger, logFormatter);

            // Initialize Dataverse client and connect
            var client = new DataverseClient(config, loggingService);
            await client.Connect();

            // Initialize CSV exporter
            var exporter = new CsvExporter(config, client, loggingService);

            // Log export parameters
            var maxItemsMessage = config.Export.MaxItemCount.HasValue
                ? $"(max {config.Export.MaxItemCount.Value:N0} records)"
                : "(no limit)";

            loggingService.LogInformation(
                "Starting export - Entity: {Entity}, View: {View} {MaxItems}",
                config.Export.Entity,
                config.Export.View,
                maxItemsMessage
            );

            // Retrieve and export data
            var data = client.RetrieveData(
                config.Export.Entity,
                config.Export.View,
                config.Export.PageSize,
                config.Export.MaxItemCount
            );

            await exporter.ExportData(data);

            loggingService.LogInformation("Export completed successfully");
        }
        catch (Exception ex)
        {
            if (loggingService != null)
            {
                loggingService.HandleError(ex);
            }
            else
            {
                // Fallback logging if logging service is not initialized
                Console.Error.WriteLine($"Critical error: {ex.Message}");
                Console.Error.WriteLine($"Details: {ex}");
            }
            Environment.Exit(1);
        }
        finally
        {
            loggerFactory?.Dispose();
        }
    }
}
