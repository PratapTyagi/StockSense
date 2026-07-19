# Entity Framework Core - Database Connection & Repository Pattern Guide

This document explains how to connect to the MySQL database using Entity Framework Core and insert data into the `stock_history` table using the **Repository Pattern** — the .NET industry standard.

---

## 1. How to Make the Database Connection

### Overview of the Layers

```
appsettings.json (connection string)
       ↓
Program.cs (registers DbContext in DI container)
       ↓
StockDataContext : DbContext (defines DbSets / table mappings)
       ↓
Entity classes (StockHistoryRecord → maps to "stock_history" table)
```

### Step-by-Step

#### A. Connection String (`appsettings.json`)

You already have this:

```json
{
  "ConnectionStrings": {
    "StockDataDb": "Server=<HOST>;Port=3306;Database=stockSenseDb;Uid=<USER>;Pwd=<PASSWORD>;"
  }
}
```

#### B. DbContext Class (`Data/StockDataContext.cs`)

The `DbContext` is EF Core's main class that:

- Represents a **session** with the database
- Exposes **DbSet<T>** properties (each maps to a table)
- Handles change tracking, migrations, and query translation

```csharp
using Microsoft.EntityFrameworkCore;
using StockDataService.Entities;

namespace StockDataService.Data;

public class StockDataContext : DbContext
{
    public StockDataContext(DbContextOptions<StockDataContext> options) : base(options) { }

    public DbSet<StockHistoryRecord> StockHistory { get; set; }
}
```

#### C. Register DbContext in DI (`Program.cs`)

Using the **Pomelo MySQL provider** (already in your `.csproj`):

```csharp
builder.Services.AddDbContext<StockDataContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);
```

**What this does:**

- Registers `StockDataContext` as a **scoped** service in the DI container
- Every HTTP request gets its own `DbContext` instance (scoped lifetime)
- EF Core manages connection pooling internally

---

## 2. How to Insert Data Using the Repository Pattern

### Why Repository Pattern?

| Benefit                    | Explanation                                        |
| -------------------------- | -------------------------------------------------- |
| **Separation of Concerns** | Business logic doesn't know about EF Core directly |
| **Testability**            | You can mock `IRepository` in unit tests           |
| **Single Responsibility**  | Each repository handles one entity/aggregate       |
| **Consistency**            | All data access follows the same pattern           |

### The Industry-Standard Architecture

```
Controller → Service (business logic) → IRepository → DbContext → Database
```

### Step-by-Step Implementation Plan

#### A. Define the Repository Interface (`Interfaces/IStockHistoryRepository.cs`)

```csharp
using StockDataService.Entities;

namespace Interfaces;

public interface IStockHistoryRepository
{
    Task InsertManyAsync(IEnumerable<StockHistoryRecord> records);
    Task<List<StockHistoryRecord>> GetBySymbolAsync(string symbol);
}
```

**Why an interface?**

- Enables dependency injection
- Allows mocking in tests
- Decouples the consumer from the implementation

#### B. Implement the Repository (`Repositories/StockHistoryRepository.cs`)

```csharp
using Microsoft.EntityFrameworkCore;
using StockDataService.Data;
using StockDataService.Entities;
using Interfaces;

namespace Repositories;

public class StockHistoryRepository : IStockHistoryRepository
{
    private readonly StockDataContext _context;

    public StockHistoryRepository(StockDataContext context)
    {
        _context = context;
    }

    public async Task InsertManyAsync(IEnumerable<StockHistoryRecord> records)
    {
        await _context.StockHistory.AddRangeAsync(records);
        await _context.SaveChangesAsync();
    }

    public async Task<List<StockHistoryRecord>> GetBySymbolAsync(string symbol)
    {
        return await _context.StockHistory
            .Where(r => r.Symbol == symbol)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }
}
```

**Key EF Core methods used:**
| Method | Purpose |
|--------|---------|
| `AddRangeAsync()` | Stages multiple entities for insertion (tracked in memory) |
| `SaveChangesAsync()` | Executes the actual `INSERT` SQL statements in a single transaction |
| `ToListAsync()` | Executes a `SELECT` query and materializes results |

#### C. Register in DI (`Program.cs` or `ServiceCollectionExtensions.cs`)

```csharp
services.AddScoped<IStockHistoryRepository, StockHistoryRepository>();
```

#### D. Use in `ZerodhaHelper` (or a Service layer)

```csharp
public class ZerodhaHelper : IZerodhaHelper
{
    private readonly IStockHistoryRepository _stockHistoryRepo;
    // ... other dependencies

    public async Task GetStockHistoricalData(string stockSymbol)
    {
        // ... fetch data from API ...

        var records = responseData.Data.Candles.Select(candle => new StockHistoryRecord
        {
            Symbol = stockSymbol,
            Timestamp = candle.Time,
            Open = candle.Open,
            High = candle.High,
            Low = candle.Low,
            Close = candle.Close,
            Volume = candle.Volume
        }).ToList();

        // Insert all records into the database
        await _stockHistoryRepo.InsertManyAsync(records);
    }
}
```

---

## 3. How to Verify Data Was Inserted

### Option A: Return count from repository

```csharp
public async Task<int> InsertManyAsync(IEnumerable<StockHistoryRecord> records)
{
    await _context.StockHistory.AddRangeAsync(records);
    return await _context.SaveChangesAsync(); // Returns number of rows affected
}
```

`SaveChangesAsync()` returns the number of state entries written to the database. If you pass 100 records and get back 100, all were inserted.

### Option B: Wrap in a transaction (for critical operations)

```csharp
public async Task InsertManyAsync(IEnumerable<StockHistoryRecord> records)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        await _context.StockHistory.AddRangeAsync(records);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Option C: Use `ExecuteSqlRawAsync` for bulk insert (performance)

For very large datasets (10,000+ rows), EF Core's `AddRangeAsync` can be slow. In that case, consider:

- **EFCore.BulkExtensions** NuGet package (provides `BulkInsertAsync`)
- Raw SQL with parameterized queries

---

## 4. Summary of Files to Create/Modify

| File                                        | Action | Purpose                                  |
| ------------------------------------------- | ------ | ---------------------------------------- |
| `Data/StockDataContext.cs`                  | Create | DbContext with DbSet<StockHistoryRecord> |
| `Interfaces/IStockHistoryRepository.cs`     | Create | Repository contract                      |
| `Repositories/StockHistoryRepository.cs`    | Create | Repository implementation                |
| `Extentions/ServiceCollectionExtensions.cs` | Modify | Register DbContext + Repository          |
| `Program.cs`                                | Modify | Add `AddDbContext` call                  |
| `Helpers/ZerodhaHelper.cs`                  | Modify | Inject & use IStockHistoryRepository     |

---

## 5. Flow Diagram

```
HTTP Request
    ↓
StockDataController.FetchHistory("RELIANCE")
    ↓
ZerodhaHelper.GetStockHistoricalData("RELIANCE")
    ↓  (fetches from Kite API)
    ↓  (maps Candle[] → StockHistoryRecord[])
    ↓
IStockHistoryRepository.InsertManyAsync(records)
    ↓
StockDataContext.StockHistory.AddRangeAsync(records)
    ↓
SaveChangesAsync() → Executes: INSERT INTO stock_history (...) VALUES (...), (...), ...
    ↓
MySQL Database (stock_history table)
```

---

## Key Takeaways

1. **DbContext** = your database session. Registered as scoped (one per request).
2. **Repository Pattern** = abstraction over DbContext. Industry standard for testability and clean architecture.
3. **`AddRangeAsync` + `SaveChangesAsync`** = the EF Core way to batch-insert records.
4. **`SaveChangesAsync` return value** = number of rows written (use this to verify).
5. **Transactions** = use when you need atomicity guarantees across multiple operations.
