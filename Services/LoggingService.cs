using Microsoft.Extensions.Logging;

namespace DataverseCsvExporter.Services;

public class LoggingService
{
    private readonly ILogger _logger;
    private readonly ILogFormatter _formatter;

    public LoggingService(ILogger logger, ILogFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }

    public void LogInformation(string message, params object[] args)
    {
        var formattedMessage = _formatter.FormatMessage(message);
        _logger.LogInformation(formattedMessage, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        var formattedMessage = _formatter.FormatMessage(message);
        _logger.LogWarning(formattedMessage, args);
    }

    public void LogError(string message, params object[] args)
    {
        var formattedMessage = _formatter.FormatMessage(message);
        _logger.LogError(formattedMessage, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        var formattedMessage = _formatter.FormatMessage(message);
        _logger.LogDebug(formattedMessage, args);
    }

    public void HandleError(Exception ex)
    {
        // Map exception types to appropriate log levels and messages
        var (level, message) = ex switch
        {
            ArgumentException argEx => (
                LogLevel.Error,
                $"Configuration error: {argEx.Message}"
            ),
            InvalidOperationException invEx => (
                LogLevel.Error,
                $"Operation error: {invEx.Message}"
            ),
            IOException ioEx => (
                LogLevel.Error,
                $"File operation error: {ioEx.Message}"
            ),
            _ => (
                LogLevel.Critical,
                $"An unexpected error occurred: {ex.Message}"
            )
        };

        // Log the error with appropriate level and structured data
        var formattedMessage = _formatter.FormatMessage("Error occurred: {ErrorMessage}. Exception type: {ExceptionType}");
        _logger.Log(
            level,
            ex,
            formattedMessage,
            message,
            ex.GetType().Name
        );

        // Log detailed exception information at debug level
        var detailedMessage = _formatter.FormatMessage("Detailed exception information: {ExceptionDetails}");
        _logger.LogDebug(
            ex,
            detailedMessage,
            ex.ToString()
        );
    }
}
