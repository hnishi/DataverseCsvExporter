using System.Text.Json.Serialization;

namespace DataverseCsvExporter.Models;

public class Configuration
{
    public Configuration()
    {
        Dataverse = new DataverseConfig();
        Export = new ExportConfig();
    }

    [JsonPropertyName("dataverse")]
    public DataverseConfig Dataverse { get; set; }

    [JsonPropertyName("export")]
    public ExportConfig Export { get; set; }
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
}
