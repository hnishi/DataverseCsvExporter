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
        ErrorHandler? errorHandler = null;

        try
        {
            // Load configuration
            var configManager = new ConfigurationManager();
            configManager.LoadConfiguration();
            var config = configManager.GetSettings();

            // Initialize logger factory
            loggerFactory = CreateLoggerFactory(config);

            // Initialize error handler
            var errorLogger = loggerFactory.CreateLogger<ErrorHandler>();
            errorHandler = new ErrorHandler(errorLogger);

            // Initialize Dataverse client and connect
            var dataverseLogger = loggerFactory.CreateLogger<DataverseClient>();
            var client = new DataverseClient(config, dataverseLogger);
            await client.Connect();

            // Initialize CSV exporter
            var exporterLogger = loggerFactory.CreateLogger<CsvExporter>();
            var exporter = new CsvExporter(config, client, exporterLogger);

            // Log export parameters
            var maxItemsMessage = config.Export.MaxItemCount.HasValue
                ? $"(max {config.Export.MaxItemCount.Value:N0} records)"
                : "(no limit)";

            errorHandler.LogInformation(
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

            errorHandler.LogInformation("Export completed successfully");
        }
        catch (Exception ex)
        {
            if (errorHandler != null)
            {
                errorHandler.HandleError(ex);
            }
            else
            {
                // Fallback logging if error handler is not initialized
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
