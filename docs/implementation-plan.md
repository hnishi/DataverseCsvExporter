# Implementation Plan for Maximum Record Limit Feature

## Overview

Add maximum record count limit functionality to the Dataverse CSV Exporter.

## Changes Required

### 1. Configuration.cs

- Add `MaxItemCount` property to `ExportConfig` class
- Add validation for `MaxItemCount` (must be greater than 0 if specified)
- Keep default value as null (unlimited)

```csharp
public class ExportConfig
{
    [JsonPropertyName("maxItemCount")]
    public int? MaxItemCount { get; set; }

    public void Validate()
    {
        if (MaxItemCount.HasValue && MaxItemCount.Value <= 0)
        {
            throw new ArgumentException("MaxItemCount must be greater than 0 if specified.");
        }
    }
}
```

### 2. Configuration Files

Update both config.json and config.template.json:

```json
{
  "export": {
    "maxItemCount": null // null means unlimited
    // other existing settings...
  }
}
```

### 3. DataverseClient.cs

- Add ILogger for logging
- Implement record count limit in RetrieveData method
- Add logging when maximum record count is reached

```csharp
public async IAsyncEnumerable<Entity> RetrieveData(string entityName, string viewName, int pageSize, int? maxItemCount = null)
{
    // ... existing code ...

    var totalRetrieved = 0;
    while (true)
    {
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

            yield return entity;
        }
    }
}
```

## Technical Details

- Null value for MaxItemCount means no limit
- Validation will ensure MaxItemCount is positive if specified
- English log messages will be used for consistency
- Existing pagination functionality remains unchanged
