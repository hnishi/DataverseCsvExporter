# パフォーマンス改善計画：NormalizeAttributes の最適化

## 概要

現在の `NormalizeAttributes` メソッドは、すべてのレコードを走査して列を特定する必要があり、パフォーマンスとメモリ使用の面で非効率です。この改善計画では、Dataverse のビュー定義から列情報を取得することで、処理を最適化します。

## 実装詳細

### 1. DataverseClient の拡張

- `savedquery` エンティティから列情報を取得する新しいメソッドを追加
- ビューのレイアウト情報をキャッシュして再利用

```csharp
public async Task<List<string>> GetViewColumns(string viewName, string entityName)
{
    var view = await GetSavedQuery(viewName, entityName);
    // fetchxmlから列情報を抽出
    return columns;
}
```

### 2. CsvExporter の最適化

- `NormalizeAttributes` メソッドを改善し、事前に取得した列情報を使用
- メモリ効率を考慮したストリーミング処理の実装

```csharp
private List<Dictionary<string, string>> NormalizeAttributes(
    IEnumerable<Dictionary<string, string>> records,
    List<string> viewColumns)
{
    return records.Select(record =>
    {
        var normalizedRecord = new Dictionary<string, string>();
        foreach (var column in viewColumns)
        {
            normalizedRecord[column] = record.ContainsKey(column) ? record[column] : string.Empty;
        }
        return normalizedRecord;
    }).ToList();
}
```

### 3. メモリ最適化

- 必要な列のみを処理
- 不要なメモリ割り当ての削減
- ストリーミング処理の活用

## 期待される効果

1. パフォーマンスの向上
   - レコード全体の走査が不要
   - 列情報が事前に判明
2. メモリ使用量の削減
   - 必要な列のみを処理
   - ストリーミング処理による効率化
3. 保守性の向上
   - ビューの定義に従った出力
   - コードの意図がより明確

## 実装手順

1. `DataverseClient` に `GetViewColumns` メソッドを追加
2. `CsvExporter` の `NormalizeAttributes` メソッドを改善
3. メモリ効率を考慮したストリーミング処理の実装
