using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using DataverseCsvExporter.Models;

namespace DataverseCsvExporter.Services;

public class CsvExporter
{
  private readonly Configuration _config;
  private readonly ILogger<CsvExporter> _logger;
  private readonly DataverseClient _client;
  private readonly DateFormatter _dateFormatter;
  private List<string>? _viewColumns;
public CsvExporter(Configuration config, DataverseClient client, ILogger<CsvExporter> logger)
{
  _config = config;
  _client = client;
  _logger = logger;
  _dateFormatter = new DateFormatter(config.Export.DateFormat);
}

  private List<Dictionary<string, string>> NormalizeAttributes(IEnumerable<Dictionary<string, string>> records)
  {
    if (_viewColumns == null)
      throw new InvalidOperationException("View columns are not initialized");

    return records.Select(record =>
    {
      var normalizedRecord = new Dictionary<string, string>();
      foreach (var column in _viewColumns)
      {
        normalizedRecord[column] = record.ContainsKey(column) ? record[column] : string.Empty;
      }
      return normalizedRecord;
    }).ToList();
  }

  public async Task ExportData(IAsyncEnumerable<Entity> entities)
  {
    // ビューの列情報を取得
    _viewColumns = await _client.GetViewColumns(_config.Export.View, _config.Export.Entity);
    _logger.LogInformation("Retrieved {Count} columns from view {View}", _viewColumns.Count, _config.Export.View);

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

    // Write header using view columns
    foreach (var column in _viewColumns!)
    {
      csv.WriteField(column);
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
          var value = FormatAttributeValue(attribute.Value, attribute.Key);
          data[attribute.Key] = value;
      }

      return data;
  }

  private string FormatAttributeValue(object value, string attributeName)
  {
    return value switch
    {
      null => string.Empty,
      EntityReference entityRef => entityRef.Name ?? entityRef.Id.ToString(),
      Money money => money.Value.ToString(CultureInfo.InvariantCulture),
      OptionSetValue optionSet => GetOptionSetDisplayName(optionSet, attributeName),
      DateTime dateTime => _dateFormatter.FormatDateTime(dateTime, IsDateOnlyAttribute(attributeName)),
      bool boolean => boolean.ToString().ToLower(),
      _ => value.ToString() ?? string.Empty
    };
  }

  private bool IsDateOnlyAttribute(string attributeName)
  {
    // Dataverseのメタデータから属性の型を取得して判定
    var attributeMetadata = _client.GetAttributeMetadata(_config.Export.Entity, attributeName);
    return attributeMetadata?.AttributeType == AttributeTypeCode.DateTime &&
           (attributeMetadata as DateTimeAttributeMetadata)?.Format == DateTimeFormat.DateOnly;
  }

  private string GetOptionSetDisplayName(OptionSetValue optionSet, string attributeName)
  {
    var label = _client.GetOptionSetLabel(_config.Export.Entity, attributeName, optionSet.Value);
    if (label == null)
    {
      _logger.LogWarning(
        "Could not find label for option set value {Value} in attribute {Attribute} of entity {Entity}",
        optionSet.Value,
        attributeName,
        _config.Export.Entity);
      return optionSet.Value.ToString();
    }
    return label;
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
