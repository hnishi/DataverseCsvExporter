using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using DataverseCsvExporter.Models;
using System.Xml.Linq;

namespace DataverseCsvExporter.Services;

public class DataverseClient
{
    private readonly string _connectionString;
    private readonly LoggingService _logger;
    private ServiceClient? _client;
    private readonly Dictionary<string, Dictionary<string, AttributeMetadata>> _metadataCache = new();
    private readonly Dictionary<(string ViewName, string EntityName), Entity> _viewCache = new();

    public DataverseClient(Configuration config, LoggingService logger)
    {
        _logger = logger;
        _connectionString = $@"
            AuthType = OAuth;
            Url = {config.Dataverse.Url};
            UserName = {config.Dataverse.Username};
            Password = {config.Dataverse.Password};
            AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
            RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
            LoginPrompt = Auto;
            RequireNewInstance = True;";
    }

    public async Task Connect()
    {
        try
        {
            _client = await Task.Run(() => new ServiceClient(_connectionString));
            if (!_client.IsReady)
            {
                throw new InvalidOperationException("Failed to connect to Dataverse.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to Dataverse: {ex.Message}", ex);
        }
    }

    public async IAsyncEnumerable<Entity> RetrieveData(string entityName, string viewName, int pageSize, int? maxItemCount = null)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected. Call Connect() first.");

        // Get the view query
        var view = await GetSavedQuery(viewName, entityName);
        if (view == null)
            throw new ArgumentException($"View '{viewName}' not found for entity '{entityName}'");

        var fetchXml = view.GetAttributeValue<string>("fetchxml");
        if (string.IsNullOrEmpty(fetchXml))
            throw new ArgumentException($"View '{viewName}' does not contain a valid FetchXML query");

        var pageNumber = 1;
        var totalRetrieved = 0;

        while (true)
        {
            var results = await HandlePagination(fetchXml, pageNumber++, pageSize);
            if (results == null || !results.Any())
                break;

            foreach (var entity in results)
            {
                totalRetrieved++;
                if (maxItemCount.HasValue && totalRetrieved > maxItemCount.Value)
                {
                    _logger.LogInformation(
                        "Maximum record count limit reached {MaxItemCount}. Stopping data retrieval.",
                        maxItemCount.Value);
                    yield break;
                }

                _logger.LogDebug(
                    "Retrieved record {Count}: Entity={EntityName}, Id={EntityId}, Attributes={AttributeCount}",
                    totalRetrieved,
                    entityName,
                    entity.Id,
                    entity.Attributes.Count);

                yield return entity;
            }

            if (results.Count < pageSize)
                break;
        }
    }

    private async Task<Entity?> GetSavedQuery(string viewName, string entityName)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected.");

        var cacheKey = (viewName, entityName);

        // Check cache first
        if (_viewCache.TryGetValue(cacheKey, out var cachedView))
            return cachedView;

        // First try to find system view
        var systemView = await GetSystemView(viewName, entityName);
        if (systemView != null)
        {
            _viewCache[cacheKey] = systemView;
            return systemView;
        }

        // If system view not found, look for personal view
        _logger.LogInformation(
            "System view '{ViewName}' not found for entity '{EntityName}'. Searching personal views...",
            viewName,
            entityName);

        var personalView = await GetPersonalView(viewName, entityName);
        if (personalView != null)
        {
            _viewCache[cacheKey] = personalView;
        }

        return personalView;
    }

    private async Task<Entity?> GetSystemView(string viewName, string entityName)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected.");

        var query = new QueryExpression("savedquery")
        {
            ColumnSet = new ColumnSet("fetchxml", "layoutxml"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, viewName),
                    new ConditionExpression("returnedtypecode", ConditionOperator.Equal, entityName)
                }
            }
        };

        var result = await Task.Run(() => _client.RetrieveMultiple(query));
        return result.Entities.FirstOrDefault();
    }

    private async Task<Entity?> GetPersonalView(string viewName, string entityName)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected.");

        var query = new QueryExpression("userquery")
        {
            ColumnSet = new ColumnSet("fetchxml", "layoutxml"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, viewName),
                    new ConditionExpression("returnedtypecode", ConditionOperator.Equal, entityName)
                }
            }
        };

        var result = await Task.Run(() => _client.RetrieveMultiple(query));
        return result.Entities.FirstOrDefault();
    }

    public async Task<List<string>> GetViewColumns(string viewName, string entityName)
    {
        var view = await GetSavedQuery(viewName, entityName);
        if (view == null)
            throw new ArgumentException($"View '{viewName}' not found for entity '{entityName}'");

        var layoutXml = view.GetAttributeValue<string>("layoutxml");
        if (string.IsNullOrEmpty(layoutXml))
            throw new ArgumentException($"View '{viewName}' does not contain a valid layout definition");

        try
        {
            var doc = XDocument.Parse(layoutXml);
            var columns = doc.Descendants("cell")
                .Select(cell => cell.Attribute("name")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)  // null許容性の警告を解消
                .ToList();

            if (!columns.Any())
                throw new ArgumentException($"No columns found in view '{viewName}'");

            return columns;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new InvalidOperationException($"Failed to parse layout XML: {ex.Message}", ex);
        }
    }

    private async Task<List<Entity>> HandlePagination(string fetchXml, int pageNumber, int pageSize)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected.");

        // Add pagination to FetchXML
        var pagingFetchXml = CreatePagingFetchXml(fetchXml, pageNumber, pageSize);
        var result = await Task.Run(() => _client.RetrieveMultiple(new FetchExpression(pagingFetchXml)));
        return result.Entities.ToList();
    }

    private string CreatePagingFetchXml(string fetchXml, int pageNumber, int pageSize)
    {
        // Add paging attributes to the fetch tag
        return fetchXml.Replace(
            "<fetch",
            $"<fetch count='{pageSize}' page='{pageNumber}'");
    }

    public AttributeMetadata? GetAttributeMetadata(string entityName, string attributeName)
    {
        if (_client == null)
            throw new InvalidOperationException("Client is not connected.");

        // キャッシュにエンティティのメタデータがあるか確認
        if (!_metadataCache.TryGetValue(entityName, out var attributeMetadata))
        {
            // エンティティのメタデータを取得
            var request = new RetrieveEntityRequest
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)_client.Execute(request);
            attributeMetadata = response.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName);
            _metadataCache[entityName] = attributeMetadata;
        }

        // 属性のメタデータを返す
        return attributeMetadata.TryGetValue(attributeName, out var attribute) ? attribute : null;
    }

    public string? GetOptionSetLabel(string entityName, string attributeName, int value)
    {
        var metadata = GetAttributeMetadata(entityName, attributeName);
        if (metadata is not EnumAttributeMetadata enumMetadata)
            return null;

        var option = enumMetadata.OptionSet?.Options.FirstOrDefault(o => o.Value == value);
        return option?.Label?.UserLocalizedLabel?.Label;
    }
}
