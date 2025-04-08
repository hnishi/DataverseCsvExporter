using Microsoft.Extensions.Logging;

namespace DataverseCsvExporter.Services;

public class ErrorHandler
{
  private readonly ILogger<ErrorHandler> _logger;

  public ErrorHandler(ILogger<ErrorHandler> logger)
  {
    _logger = logger;
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
    _logger.Log(
        level,
        ex,
        "Error occurred: {ErrorMessage}. Exception type: {ExceptionType}",
        message,
        ex.GetType().Name
    );

    // Log detailed exception information at debug level
    _logger.LogDebug(
        ex,
        "Detailed exception information: {ExceptionDetails}",
        ex.ToString()
    );
  }

  public void LogInformation(string message, params object[] args)
  {
    _logger.LogInformation(message, args);
  }

  public void LogWarning(string message, params object[] args)
  {
    _logger.LogWarning(message, args);
  }

  public void LogError(string message, params object[] args)
  {
    _logger.LogError(message, args);
  }

  public void LogDebug(string message, params object[] args)
  {
    _logger.LogDebug(message, args);
  }
}
