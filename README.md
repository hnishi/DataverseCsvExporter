# Dataverse CSV Exporter

A console application that exports data from Dynamics 365 Sales Dataverse to CSV by specifying entity name and view name.

> **Note:**
> 日本語版ドキュメントは[こちら](README.ja.md)をご覧ください。

## Features

- Flexible configuration using JSON settings file
- Authentication and connection with Dataverse API
- Efficient retrieval of large data sets through pagination
- Streaming output to CSV files
- Error handling and logging

## Requirements

- .NET 8.0 SDK
- Access rights to Dynamics 365 Sales environment
- User account with Azure AD authentication enabled

## Build Instructions

1. Clone the repository

```bash
git clone [repository-url]
cd DataverseCsvExporter
```

2. Restore dependencies

```bash
dotnet restore
```

3. Build

```bash
dotnet build
```

## Creating Standalone Binary

Instructions for creating standalone binary files for Windows environment.

1. Build for x64 architecture

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

2. Build for x86 architecture

```bash
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

3. Build for ARM64 architecture

```bash
dotnet publish -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

The built binaries are generated in the following directories:

- x64: `bin/Release/net8.0/win-x64/publish/DataverseCsvExporter.exe`
- x86: `bin/Release/net8.0/win-x86/publish/DataverseCsvExporter.exe`
- ARM64: `bin/Release/net8.0/win-arm64/publish/DataverseCsvExporter.exe`

Notes:

- The generated EXE files can run independently without .NET Runtime
- config.json must be in the same directory as the executable file
- Write permissions to the output directory (default: ./output) are required

## Setup

1. Prepare configuration file

```bash
cp config.template.json config.json
```

2. Edit `config.json` with the following settings:

- Dataverse connection information
  - `url`: Dynamics 365 environment URL
    - Format: `https://[your-org].crm.dynamics.com`
    - Example: `https://contoso.crm.dynamics.com`
  - `username`: Username (email address)
    - Format: `user@your-domain.com`
    - Example: `john.doe@contoso.com`
  - `password`: Password
- Export settings
  - `entity`: Entity name to export
  - `view`: View name to use
  - `output`: Output settings
    - `directory`: Output directory
    - `fileName`: Output filename ({entity} and {timestamp} are automatically replaced)
  - `pageSize`: Number of records to retrieve per request

Configuration example:

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

## Execution

1. Run the application

```bash
dotnet run
```

2. Check the results

- Exported CSV files are saved in the `output` directory
- Filenames are generated in the format `{entity}_{timestamp}.csv`
  - Example: `account_20250407141023.csv`
- If an error occurs, an error message will be displayed in the console

## Authentication

This application connects to Dataverse using OAuth authentication. Authentication proceeds as follows:

1. Authentication with username and password
2. Token acquisition through Azure AD
3. Access to Dataverse API

Authentication notes:

- Account must support Azure AD authentication
- If multi-factor authentication (MFA) is enabled, consider using an application password
- User account must have appropriate permissions

## Error Handling

Main error messages and troubleshooting:

1. Configuration Errors

   - When configuration file is not found
   - When required items are not set
   - ⇒ Please check the contents of config.json

2. Connection Errors

   - When connection to Dataverse fails
   - ⇒ Please check:
     - If URL is in correct format (https://[your-org].crm.dynamics.com)
     - If username is in correct email address format
     - If password is correct
     - If account supports Azure AD authentication
     - If using application password when MFA is enabled

3. Data Retrieval Errors

   - When entity or view does not exist
   - When lacking access permissions
   - ⇒ Please check entity name, view name, and user permissions

4. File Operation Errors
   - When lacking write permissions to output directory
   - ⇒ Please check output directory path and permissions

## Troubleshooting

1. Authentication Errors

   - Check application registration in Azure Portal
   - Check user permissions
   - Check MFA settings

2. Data Retrieval Issues

   - Check case sensitivity of view name and entity name
   - Verify user has access to the view

3. Performance Issues
   - Adjust pageSize value (default: 5000)
   - Check network connection
