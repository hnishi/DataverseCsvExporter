using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using DataverseCsvExporter.Models;

namespace DataverseCsvExporter.Services;

public class CsvExporter
{
  private readonly Configuration _config;
  private readonly ILogger<CsvExporter> _logger;

  public CsvExporter(Configuration config, ILogger<CsvExporter> logger)
  {
    _config = config;
    _logger = logger;
  }

  private List<Dictionary<string, string>> NormalizeAttributes(IEnumerable<Dictionary<string, string>> records)
  {
    var allKeys = records.SelectMany(record => record.Keys).Distinct().ToList();

    return records.Select(record =>
    {
      var normalizedRecord = new Dictionary<string, string>();
      foreach (var key in allKeys)
      {
        normalizedRecord[key] = record.ContainsKey(key) ? record[key] : string.Empty;
      }
      return normalizedRecord;
    }).ToList();
  }

  public async Task ExportData(IAsyncEnumerable<Entity> entities)
  {
    var outputPath = GetOutputPath();
    EnsureOutputDirectory(outputPath);

    var encoding = new System.Text.UTF8Encoding(true);
    await using var writer = new StreamWriter(outputPath, false, encoding);
    if (_config.Export.Output.UseBom)
    {
      await writer.BaseStream.WriteAsync(encoding.GetPreamble());
    }
    await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

    var allRecords = new List<Dictionary<string, string>>();

    await foreach (var entity in entities)
    {
      var formattedData = FormatData(entity);
      allRecords.Add(formattedData);
    }

    var normalizedRecords = NormalizeAttributes(allRecords);

    // Write header
    var allKeys = normalizedRecords.First().Keys;
    foreach (var key in allKeys)
    {
      csv.WriteField(key);
    }
    csv.NextRecord();

    // Write records
    foreach (var record in normalizedRecords)
    {
      foreach (var value in record.Values)
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
