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

    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    // Log the error with appropriate level and structured data
    _logger.Log(
        level,
        ex,
        "[{Timestamp}] Error occurred: {ErrorMessage}. Exception type: {ExceptionType}",
        timestamp,
        message,
        ex.GetType().Name
    );

    // Log detailed exception information at debug level
    _logger.LogDebug(
        ex,
        "[{Timestamp}] Detailed exception information: {ExceptionDetails}",
        timestamp,
        ex.ToString()
    );
  }

  public void LogInformation(string message, params object[] args)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    _logger.LogInformation($"[{timestamp}] {message}", args);
  }

  public void LogWarning(string message, params object[] args)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    _logger.LogWarning($"[{timestamp}] {message}", args);
  }

  public void LogError(string message, params object[] args)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    _logger.LogError($"[{timestamp}] {message}", args);
  }

  public void LogDebug(string message, params object[] args)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    _logger.LogDebug($"[{timestamp}] {message}", args);
  }
}
