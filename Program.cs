using DataverseCsvExporter.Models;
using DataverseCsvExporter.Services;

namespace DataverseCsvExporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // 設定の読み込み
            var configManager = new ConfigurationManager();
            configManager.LoadConfiguration();
            var config = configManager.GetSettings();

            // Dataverse クライアントの初期化と接続
            var client = new DataverseClient(config);
            await client.Connect();

            // CSV エクスポーターの初期化
            var exporter = new CsvExporter(config);

            // データの取得とエクスポート
            var data = client.RetrieveData(
                config.Export.Entity,
                config.Export.View,
                config.Export.PageSize);

            ErrorHandler.LogToConsole($"エクスポートを開始します: エンティティ={config.Export.Entity}, ビュー={config.Export.View}");

            await exporter.ExportData(data);

            ErrorHandler.LogToConsole("エクスポートが完了しました。");
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleError(ex);
            Environment.Exit(1);
        }
    }
}
