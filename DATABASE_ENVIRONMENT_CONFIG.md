# Database Environment Configuration

## Overview
The application uses environment-specific database connections managed in a single `appsettings.json` file:

- **Development/Local**: Uses `study-hub` database on `ep-raspy-sun-a16mmkaa-pooler`
- **Production**: Uses `study-hub-dev` database on `ep-curly-queen-a18qyk7o-pooler`

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-raspy-sun-a16mmkaa-pooler.ap-southeast-1.aws.neon.tech;Database=study-hub;Username=neondb_owner;Password=npg_JHQYeo86TaUR",
    "ProductionConnection": "Host=ep-curly-queen-a18qyk7o-pooler.ap-southeast-1.aws.neon.tech;Database=study-hub-dev;Username=neondb_owner;Password=npg_JHQYeo86TaUR"
  }
}
```

### Program.cs Logic
```csharp
// Database Configuration
var connectionString = builder.Environment.IsProduction()
    ? builder.Configuration.GetConnectionString("ProductionConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MapEnum<SessionStatus>("session_status");
    }));
```

## How It Works

The application determines which connection string to use based on the `ASPNETCORE_ENVIRONMENT` variable:
- If `ASPNETCORE_ENVIRONMENT=Production` → uses **ProductionConnection**
- Otherwise → uses **DefaultConnection**

## Deployment

### For Render.com (or any production server):

1. Set the environment variable in your deployment settings:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ```

2. The app will automatically use the production database connection from `appsettings.json`

### Local Development:

- No changes needed
- Run normally with `dotnet run` or from your IDE
- Uses the development database by default
- Default environment is "Development" when `ASPNETCORE_ENVIRONMENT` is not set

## Testing Production Configuration Locally

To test the production configuration on your local machine:

### macOS/Linux:
```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

### Windows PowerShell:
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run
```

### Windows CMD:
```cmd
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## Verification

Check which environment and connection is being used by looking at the startup logs:

```
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production  <-- Shows current environment
```

You can also add logging in `Program.cs` to verify:
```csharp
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Using Database: {(builder.Environment.IsProduction() ? "Production" : "Development")}");
```

## Summary of Changes

✅ Added `ProductionConnection` to `appsettings.json`  
✅ Updated `Program.cs` to switch connection based on environment  
✅ Removed `appsettings.Production.json` (using single config file only)  
✅ Development environment unchanged  
✅ Production environment automatically uses new database  

## Security Notes

⚠️ **Important**: For production deployments, consider using environment variables instead of hardcoding credentials:

1. In Render.com, add environment variable:
   ```
   DATABASE_URL=Host=ep-curly-queen-a18qyk7o-pooler.ap-southeast-1.aws.neon.tech;Database=study-hub-dev;Username=neondb_owner;Password=npg_JHQYeo86TaUR
   ```

2. Update `Program.cs`:
   ```csharp
   var connectionString = builder.Environment.IsProduction()
       ? Environment.GetEnvironmentVariable("DATABASE_URL") ?? builder.Configuration.GetConnectionString("ProductionConnection")
       : builder.Configuration.GetConnectionString("DefaultConnection");
   ```

This keeps sensitive credentials out of your codebase.

