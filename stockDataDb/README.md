# StockSenseDb — SQL Server Database Project

This is a **Microsoft.Build.SQL** database project that produces a `.dacpac` file for declarative schema deployment to SQL Server (local) or Azure SQL (remote).

---

## Prerequisites

### 1. Install `sqlpackage` CLI

```bash
# macOS (via .NET tool)
dotnet tool install -g microsoft.sqlpackage

# Verify installation
sqlpackage /Version
```

### 2. Local SQL Server (Docker)

Since macOS doesn't support SQL Server natively, use the Azure SQL Edge Docker image:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourPassword>" \
  -p 1433:1433 --name stocksense-sqlserver \
  -d mcr.microsoft.com/azure-sql-edge:latest
```

> **Note:** Password must meet SQL Server complexity requirements (uppercase, lowercase, number, special char, min 8 chars).

---

## Build the DACPAC

```bash
cd StockSenseDb
dotnet build
```

Output: `bin/Debug/StockSenseDb.dacpac`

---

## Deploy Locally

Deploy to your local SQL Server (Docker container):

```bash
sqlpackage /Action:Publish \
  /SourceFile:"bin/Debug/StockSenseDb.dacpac" \
  /TargetConnectionString:"Server=localhost,1433;Database=StockSenseDb;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;Encrypt=False;"
```

### What this does:

- Creates the `StockSenseDb` database if it doesn't exist
- Creates/updates all tables (`WatchListItems`, `StockHistory`)
- Runs the **Pre-Deployment** script before schema changes
- Applies schema diff (only changes what's different)
- Runs the **Post-Deployment** script (seeds data)

---

## Deploy to Azure SQL (Remote)

```bash
sqlpackage /Action:Publish \
  /SourceFile:"bin/Debug/StockSenseDb.dacpac" \
  /TargetConnectionString:"Server=<your-server>.database.windows.net;Database=StockSenseDb;User Id=<admin-user>;Password=<admin-password>;Encrypt=True;TrustServerCertificate=False;"
```

Replace:

- `<your-server>` — Your Azure SQL server name
- `<admin-user>` — SQL admin username
- `<admin-password>` — SQL admin password

---

## Generate a Diff Script (without applying)

To preview what changes will be made without actually deploying:

```bash
sqlpackage /Action:Script \
  /SourceFile:"bin/Debug/StockSenseDb.dacpac" \
  /TargetConnectionString:"Server=localhost,1433;Database=StockSenseDb;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;" \
  /DeployScriptPath:"deploy-script.sql"
```

This generates a `.sql` file you can review before executing.

---

## CI/CD Integration

In your pipeline (GitHub Actions, Azure DevOps, etc.):

```yaml
# Example GitHub Actions step
- name: Build DACPAC
  run: dotnet build StockSenseDb/StockSenseDb.sqlproj

- name: Deploy to Azure SQL
  run: |
    sqlpackage /Action:Publish \
      /SourceFile:StockSenseDb/bin/Debug/StockSenseDb.dacpac \
      /TargetConnectionString:"${{ secrets.AZURE_SQL_CONNECTION_STRING }}"
```

---

## Connection Strings for Application

After deployment, use these connection strings in your app:

**Local (appsettings.Development.json):**

```json
{
  "ConnectionStrings": {
    "StockSenseDb": "Server=localhost,1433;Database=StockSenseDb;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;"
  }
}
```

**Remote / Azure (appsettings.json):**

```json
{
  "ConnectionStrings": {
    "StockSenseDb": "Server=<your-server>.database.windows.net;Database=StockSenseDb;User Id=<user>;Password=<password>;Encrypt=True;"
  }
}
```

---

## Project Structure

```
StockSenseDb/
├── StockSenseDb.sqlproj              # SQL project file (Microsoft.Build.Sql SDK 2.2.0)
├── Tables/
│   ├── WatchListItems.sql            # Declarative table schema
│   └── StockHistory.sql              # Declarative table schema
└── Scripts/
    ├── Script.PreDeployment.sql      # Runs before schema changes
    └── Script.PostDeployment.sql     # Runs after (seed data)
```
