# Dataverse CSV Exporter

Dynamics 365 Sales の Dataverse からエンティティ名とビュー名を指定してデータを CSV にエクスポートするコンソールアプリケーションです。

## 機能

- JSON 設定ファイルによる柔軟な構成
- Dataverse API との認証と接続
- ページネーション処理による大量データの効率的な取得
- CSV ファイルへのストリーミング出力
- エラーハンドリングとログ出力

## 必要要件

- .NET 8.0 SDK
- Dynamics 365 Sales 環境へのアクセス権限
- Azure AD 認証が有効なユーザーアカウント

## ビルド手順

1. リポジトリのクローン

```bash
git clone [repository-url]
cd DataverseCsvExporter
```

2. 依存パッケージのリストア

```bash
dotnet restore
```

3. ビルド

```bash
dotnet build
```

## 実行準備

1. 設定ファイルの準備

```bash
cp config.template.json config.json
```

2. `config.json` を編集し、以下の設定を行う

- Dataverse の接続情報
  - `url`: Dynamics 365 環境の URL
    - 形式: `https://[your-org].crm.dynamics.com`
    - 例: `https://contoso.crm.dynamics.com`
  - `username`: ユーザー名（E メールアドレス）
    - 形式: `user@your-domain.com`
    - 例: `john.doe@contoso.com`
  - `password`: パスワード
- エクスポート設定
  - `entity`: エクスポートするエンティティ名
  - `view`: 使用するビュー名
  - `output`: 出力設定
    - `directory`: 出力ディレクトリ
    - `fileName`: 出力ファイル名（{entity}と{timestamp}は自動で置換）
  - `pageSize`: 1 回のリクエストで取得するレコード数

設定例：

```json
{
  "dataverse": {
    "url": "https://contoso.crm.dynamics.com",
    "username": "john.doe@contoso.com",
    "password": "your-password"
  },
  "export": {
    "entity": "account",
    "view": "active-accounts",
    "output": {
      "directory": "./output",
      "fileName": "{entity}_{timestamp}.csv"
    },
    "pageSize": 5000
  }
}
```

## スタンドアローンバイナリの作成

Windows および macOS 環境で実行可能なスタンドアローンバイナリを作成する手順です。

### Windows 向けビルド手順

1. x64 アーキテクチャ向けビルド

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

2. x86 アーキテクチャ向けビルド

```bash
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

3. ARM64 アーキテクチャ向けビルド

```bash
dotnet publish -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

Windows 向けのビルドされたバイナリは以下のディレクトリに生成されます：

- x64: `bin/Release/net8.0/win-x64/publish/DataverseCsvExporter.exe`
- x86: `bin/Release/net8.0/win-x86/publish/DataverseCsvExporter.exe`
- ARM64: `bin/Release/net8.0/win-arm64/publish/DataverseCsvExporter.exe`

### macOS 向けビルド手順

1. Intel Mac（x64）向けビルド

```bash
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

2. Apple Silicon Mac（ARM64）向けビルド

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

macOS 向けのビルドされたバイナリは以下のディレクトリに生成されます：

- Intel Mac: `bin/Release/net8.0/osx-x64/publish/DataverseCsvExporter`
- Apple Silicon: `bin/Release/net8.0/osx-arm64/publish/DataverseCsvExporter`

注意事項：

- 生成されたバイナリは.NET Runtime に依存せず、単独で実行可能です
- アプリケーションの実行には、config.json が実行ファイルと同じディレクトリに必要です
- 出力ディレクトリ（デフォルト: ./output）への書き込み権限が必要です

## 実行手順

1. アプリケーションの実行

```bash
dotnet run
```

2. 実行結果の確認

- エクスポートされた CSV ファイルは`output`ディレクトリに保存されます
- ファイル名は`{entity}_{timestamp}.csv`の形式で生成されます
  - 例：`account_20250407141023.csv`
- エラーが発生した場合は、コンソールにエラーメッセージが表示されます

## 認証について

このアプリケーションは、OAuth 認証を使用して Dataverse に接続します。認証は以下の手順で行われます：

1. ユーザー名とパスワードによる認証
2. Azure AD を介したトークンの取得
3. Dataverse API へのアクセス

認証に関する注意事項：

- アカウントは Azure AD 認証に対応している必要があります
- 多要素認証（MFA）が有効な場合は、アプリケーションパスワードの使用を検討してください
- ユーザーアカウントに適切な権限が付与されている必要があります

## エラーハンドリング

主なエラーメッセージと対処方法：

1. 設定エラー

   - 設定ファイルが見つからない場合
   - 必須項目が未設定の場合
   - ⇒ config.json の内容を確認してください

2. 接続エラー

   - Dataverse への接続に失敗した場合
   - ⇒ 以下を確認してください：
     - URL が正しい形式（https://[your-org].crm.dynamics.com）か
     - ユーザー名が正しい E メールアドレス形式か
     - パスワードが正しいか
     - アカウントが Azure AD 認証に対応しているか
     - 多要素認証が有効な場合、アプリケーションパスワードを使用しているか

3. データ取得エラー

   - エンティティやビューが存在しない場合
   - アクセス権限がない場合
   - ⇒ エンティティ名、ビュー名、およびユーザーの権限を確認してください

4. ファイル操作エラー
   - 出力ディレクトリへの書き込み権限がない場合
   - ⇒ 出力ディレクトリのパスと権限を確認してください

## トラブルシューティング

1. 認証エラーが発生する場合

   - Azure Portal でアプリケーションの登録を確認
   - ユーザーの権限を確認
   - 多要素認証の設定を確認

2. データが取得できない場合

   - ビュー名とエンティティ名の大文字小文字を確認
   - ユーザーがビューにアクセスできることを確認

3. パフォーマンスの問題
   - pageSize の値を調整（デフォルト: 5000）
   - ネットワーク接続を確認
