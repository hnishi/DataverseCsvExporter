using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Extensions.Logging;
using DataverseCsvExporter.Models;

namespace DataverseCsvExporter.Services;

public class DataverseClient
{
    private readonly string _connectionString;
    private readonly ILogger<DataverseClient> _logger;
    private ServiceClient? _client;

    public DataverseClient(Configuration config, ILogger<DataverseClient> logger)
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
                        "Maximum record count limit reached ({MaxItemCount}). Stopping data retrieval.",
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

        var query = new QueryExpression("savedquery")
        {
            ColumnSet = new ColumnSet("fetchxml"),
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
}
