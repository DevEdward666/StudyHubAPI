# Local Print Server for Render.com Deployment

This is a simple local print server that runs on your machine (with the physical printer) and processes print jobs from your Render.com backend.

## How It Works

```
[Render.com Backend] → [Database Queue] ← [This Local Server]
                                              ↓
                                      [Your USB Printer]
```

1. Render.com backend adds print jobs to database queue
2. This local server polls the database every 5 seconds
3. Processes pending print jobs
4. Prints to your local USB printer
5. Updates job status in database

## Setup

### 1. Configure Connection String

Create `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-render-postgres-url;Database=studyhub;Username=user;Password=pass;SSL Mode=Require"
  },
  "PrintServer": {
    "PollIntervalSeconds": 5,
    "MaxRetries": 3,
    "PrintTimeout": 15000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

Get your Render.com PostgreSQL connection string from:
- Render Dashboard → Your Database → Internal Database URL

### 2. Run the Server

```bash
# Install dependencies (first time only)
dotnet restore

# Run the print server
dotnet run

# Or build and run
dotnet build
dotnet run --no-build
```

### 3. Keep It Running

**Option A: Run in terminal (development)**
```bash
dotnet run
```

**Option B: Run as background service (production)**

**macOS (launchd):**
Create `~/Library/LaunchAgents/com.studyhub.printserver.plist`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.studyhub.printserver</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/share/dotnet/dotnet</string>
        <string>run</string>
        <string>--project</string>
        <string>/path/to/LocalPrintServer</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>/tmp/printserver.log</string>
    <key>StandardErrorPath</key>
    <string>/tmp/printserver.error.log</string>
</dict>
</plist>
```

Then:
```bash
launchctl load ~/Library/LaunchAgents/com.studyhub.printserver.plist
```

**Linux (systemd):**
Create `/etc/systemd/system/studyhub-print.service`:
```ini
[Unit]
Description=StudyHub Print Server
After=network.target

[Service]
Type=simple
User=youruser
WorkingDirectory=/path/to/LocalPrintServer
ExecStart=/usr/bin/dotnet run
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Then:
```bash
sudo systemctl enable studyhub-print
sudo systemctl start studyhub-print
sudo systemctl status studyhub-print
```

**Windows (NSSM):**
```powershell
# Download NSSM from https://nssm.cc/
nssm install StudyHubPrintServer "C:\Program Files\dotnet\dotnet.exe" "run --project C:\path\to\LocalPrintServer"
nssm start StudyHubPrintServer
```

## Verify It's Working

### Check Printer Detection
```bash
cd /path/to/StudyHubAPI
./diagnose-usb-printer-server.sh
```

### Check Logs
```bash
# Watch the console output
# You should see:
🔄 Print server started. Polling every 5 seconds...
✅ Found 0 pending print jobs
```

### Test End-to-End
1. From Render.com frontend/API, create a print job
2. Watch local server logs - should see:
   ```
   ✅ Found 1 pending print jobs
   🖨️ Processing print job: abc-123-def
   ✅ Print completed successfully
   ```
3. Physical receipt should print!

## Troubleshooting

### "No printer found"
```bash
./diagnose-usb-printer-server.sh
# Follow the recommendations
```

### "Cannot connect to database"
- Check connection string in `appsettings.json`
- Make sure Render.com database allows external connections
- Check firewall settings

### "Jobs stuck in Processing"
```bash
# Reset stuck jobs in database
psql $DATABASE_URL
UPDATE "PrintJobs" SET "Status" = 'Pending' WHERE "Status" = 'Processing';
```

### "Printer disconnects"
- Make sure printer is powered on
- Check USB cable
- See `USB_PRINTER_SERVER_DEPLOYMENT.md` for detailed troubleshooting

## Files Needed

To set up local print server, you need:

1. **This README** - Setup instructions
2. **Program.cs** - Main application (see LocalPrintServer/Program.cs)
3. **appsettings.json** - Configuration
4. **LocalPrintServer.csproj** - Project file
5. **ThermalPrinterService.cs** - Copied from main backend
6. **Models/** - Copied from main backend (PrintJob, ReceiptDto, etc.)

## Quick Start Script

```bash
#!/bin/bash
# quick-start-print-server.sh

echo "Starting StudyHub Local Print Server..."

# Check printer
echo "Checking printer connection..."
./diagnose-usb-printer-server.sh

# Start server
echo "Starting print server..."
dotnet run

# Server will start and poll for jobs every 5 seconds
```

## Security Notes

⚠️ **Important:**
- Keep your database connection string secure
- Don't commit `appsettings.json` with real credentials to git
- Use environment variables for production:
  ```bash
  export ConnectionStrings__DefaultConnection="your-db-url"
  dotnet run
  ```

## Monitoring

Watch logs for:
- ✅ Print jobs completed
- ⚠️ Print failures
- 🔄 Connection status
- 📊 Job statistics

## Support

See main documentation:
- `USB_PRINTER_SERVER_DEPLOYMENT.md`
- `RENDER_DEPLOYMENT_PRINTING.md`
- `USB_PRINTER_QUICK_FIX.md`

