using System.Globalization;
using CsvHelper;
using Microsoft.Xrm.Sdk;
using DataverseCsvExporter.Models;

namespace DataverseCsvExporter.Services;

public class CsvExporter
{
  private readonly Configuration _config;

  public CsvExporter(Configuration config)
  {
    _config = config;
  }

  public async Task ExportData(IAsyncEnumerable<Entity> entities)
  {
    var outputPath = GetOutputPath();
    EnsureOutputDirectory(outputPath);

    await using var writer = new StreamWriter(outputPath);
    await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

    bool headerWritten = false;

    await foreach (var entity in entities)
    {
      var formattedData = FormatData(entity);

      if (!headerWritten)
      {
        foreach (var key in formattedData.Keys)
        {
          csv.WriteField(key);
        }
        csv.NextRecord();
        headerWritten = true;
      }

      foreach (var value in formattedData.Values)
      {
        csv.WriteField(value);
      }
      csv.NextRecord();
    }
  }

  private Dictionary<string, string> FormatData(Entity entity)
  {
    var data = new Dictionary<string, string>();

    foreach (var attribute in entity.Attributes)
    {
      var value = FormatAttributeValue(attribute.Value);
      data[attribute.Key] = value;
    }

    return data;
  }

  private string FormatAttributeValue(object value)
  {
    return value switch
    {
      null => string.Empty,
      EntityReference entityRef => entityRef.Name ?? entityRef.Id.ToString(),
      Money money => money.Value.ToString(CultureInfo.InvariantCulture),
      OptionSetValue optionSet => optionSet.Value.ToString(),
      DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
      bool boolean => boolean.ToString().ToLower(),
      _ => value.ToString() ?? string.Empty
    };
  }

  private string GetOutputPath()
  {
    var fileName = _config.Export.Output.FileName
        .Replace("{entity}", _config.Export.Entity)
        .Replace("{timestamp}", DateTime.Now.ToString("yyyyMMddHHmmss"));

    return Path.Combine(_config.Export.Output.Directory, fileName);
  }

  private void EnsureOutputDirectory(string outputPath)
  {
    var directory = Path.GetDirectoryName(outputPath);
    if (directory != null && !Directory.Exists(directory))
    {
      Directory.CreateDirectory(directory);
    }
  }
}
