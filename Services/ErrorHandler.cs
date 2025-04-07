namespace DataverseCsvExporter.Services;

public class ErrorHandler
{
  public static void HandleError(Exception ex)
  {
    var errorMessage = ex switch
    {
      ArgumentException argEx => $"設定エラー: {argEx.Message}",
      InvalidOperationException invEx => $"操作エラー: {invEx.Message}",
      IOException ioEx => $"ファイル操作エラー: {ioEx.Message}",
      _ => $"予期せぬエラーが発生しました: {ex.Message}"
    };

    LogToConsole(errorMessage);
    LogToConsole($"詳細: {ex}");
  }

  public static void LogToConsole(string message)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    Console.Error.WriteLine($"[{timestamp}] {message}");
  }
}
