namespace DataverseCsvExporter.Models;

public class Configuration
{
    public DataverseConfig Dataverse { get; set; } = null!;
    public ExportConfig Export { get; set; } = null!;
}

public class DataverseConfig
{
    public string Url { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class ExportConfig
{
    public string Entity { get; set; } = null!;
    public string View { get; set; } = null!;
    public OutputConfig Output { get; set; } = null!;
    public int PageSize { get; set; } = 5000;
}

public class OutputConfig
{
    public string Directory { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
