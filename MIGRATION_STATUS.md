# Migration Status and Instructions

## Current Situation

The database has a notification table created, but there are migration conflicts. Here's how to resolve them:

## Option 1: Manual Migration Sync (Recommended)

Since the notifications table already exists in the database, we need to mark the migration as applied:

```bash
cd Study-Hub

# Connect to your PostgreSQL database and run:
# INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
# VALUES ('20251024000000_AddNotifications', '9.0.9');

# Then apply remaining migrations:
dotnet ef database update
```

## Option 2: Fresh Migration

If you want to start clean with migrations:

```bash
cd Study-Hub

# Remove all pending migrations
dotnet ef migrations remove
dotnet ef migrations remove  # repeat as needed

# Create new migration with all changes
dotnet ef migrations add CompleteSchema

# Apply to database
dotnet ef database update
```

## Option 3: Suppress Warning Temporarily

Add this to `ApplicationDbContext.cs` OnConfiguring method (NOT RECOMMENDED for production):

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
}
```

## Web Push Setup Steps

1. **Migration is Created**: AddPushSubscriptions migration is ready
2. **Apply Migration**:
   ```bash
   cd Study-Hub
   dotnet ef database update
   ```

3. **Verify Table**:
   ```sql
   SELECT * FROM push_subscriptions LIMIT 1;
   ```

4. **Test API**:
   ```bash
   # Get VAPID public key
   curl http://localhost:5212/api/push/vapid-public-key
   ```

## Migration Files

Current migrations (in order):
1. `20251022124231_InitialCreate` - ✅ Applied
2. `20251022135957_updateStatus` - ✅ Applied
3. `20251024000000_AddNotifications` - ⚠️ Pending (table exists)
4. `20251024140003_AddNotifications2` - ⚠️ Pending (conflict)
5. `20251024144240_AddPushSubscriptions` - ⚠️ Pending (ready to apply)

## Quick Fix Command

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub

# Remove conflicting migration
dotnet ef migrations remove

# This should leave only AddPushSubscriptions
# Then update database
dotnet ef database update
```

## Notes

- The notifications table already exists in the database
- We added PushSubscription entity and need to create its table
- VAPID keys are already configured in appsettings.json
- All services are registered in Program.cs

## Testing After Migration

```bash
# Run the application
dotnet run

# In another terminal, test the endpoint
curl http://localhost:5212/api/push/vapid-public-key
```

Expected response:
```json
{
  "publicKey": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEW0NeHAc8htI..."
}
```

