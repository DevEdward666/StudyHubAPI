# Render.com Deployment - Quick Setup Guide

## 🎯 Goal
Deploy your StudyHub backend to Render.com and handle printing via a local print server.

---

## 📋 Quick Setup (3 Steps)

### Step 1: Deploy Backend to Render.com (5 minutes)

1. **Push your code to GitHub**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI
   git add .
   git commit -m "Add print queue support"
   git push origin main
   ```

2. **Create PostgreSQL Database on Render.com**
   - Go to https://render.com
   - Click "New +" → "PostgreSQL"
   - Name: `studyhub-db`
   - Click "Create Database"
   - Copy the **Internal Database URL**

3. **Deploy Backend Service**
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Name: `studyhub-api`
   - Build Command: `dotnet restore && dotnet build`
   - Start Command: `dotnet run --project Study-Hub/Study-Hub.csproj`
   - Add Environment Variable:
     - Key: `ConnectionStrings__DefaultConnection`
     - Value: (paste the Internal Database URL from step 2)
   - Click "Create Web Service"

4. **Run Migrations**
   ```bash
   # After deployment, run migrations
   # Option A: In Render.com shell
   dotnet ef database update

   # Option B: Locally against Render DB
   export ConnectionStrings__DefaultConnection="your-render-db-url"
   dotnet ef database update --project Study-Hub
   ```

✅ **Backend is now live on Render.com!**

---

### Step 2: Set Up Local Print Server (10 minutes)

1. **Copy diagnostic script to your local machine**
   ```bash
   # You already have it at:
   /Users/edward/Documents/StudyHubAPI/diagnose-usb-printer-server.sh
   ```

2. **Check your printer connection**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI
   ./diagnose-usb-printer-server.sh
   ```

   Fix any issues shown (USB connection, permissions, etc.)

3. **Configure Local Print Server**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/LocalPrintServer
   
   # Edit appsettings.json
   nano appsettings.json
   ```

   Replace with your Render.com database URL:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "YOUR_RENDER_POSTGRES_INTERNAL_URL_HERE"
     },
     "PrintServer": {
       "PollIntervalSeconds": 5,
       "MaxRetries": 3,
       "PrintTimeout": 15000
     }
   }
   ```

4. **Run Local Print Server**
   ```bash
   cd LocalPrintServer
   dotnet restore
   dotnet run
   ```

   You should see:
   ```
   ╔════════════════════════════════════════════════════════╗
   ║       StudyHub Local Print Server                     ║
   ║       Processes print jobs from Render.com            ║
   ╚════════════════════════════════════════════════════════╝

   🚀 Print server started
   ⏱️  Polling interval: 5 seconds
   🔄 Max retries: 3
   ```

✅ **Local print server is running!**

---

### Step 3: Register Print Queue Service (2 minutes)

Update your Render.com backend to use the print queue.

1. **Register service in Program.cs (or Startup.cs)**

   Find where services are registered and add:
   ```csharp
   // Add print queue service
   services.AddScoped<IPrintQueueService, PrintQueueService>();
   ```

2. **Use print queue in your controller**

   Find your print endpoint and modify it:
   ```csharp
   // OLD CODE (won't work on Render.com)
   // await _thermalPrinterService.PrintReceiptAsync(receipt);

   // NEW CODE (works with Render.com)
   var jobId = await _printQueueService.QueuePrintJobAsync(
       receipt, 
       sessionId: session.Id,
       userId: session.UserId,
       priority: 5
   );

   return Ok(new { 
       success = true, 
       message = "Print job queued", 
       jobId = jobId 
   });
   ```

3. **Redeploy to Render.com**
   ```bash
   git add .
   git commit -m "Use print queue for Render.com"
   git push origin main
   ```

   Render.com will auto-deploy.

✅ **Everything is connected!**

---

## 🧪 Test the Complete Flow

1. **Make a print request from your app**
   - User ends a session
   - Backend adds print job to database queue
   - Returns: `{ success: true, jobId: "abc-123" }`

2. **Watch local print server logs**
   ```
   📋 Found 1 pending print job(s)
   🖨️  Processing print job: abc-123
   ✅ Port opened successfully, sending 1234 bytes...
   ✅ Print job abc-123 completed successfully
   ```

3. **Physical receipt prints! 🎉**

---

## 📂 File Locations

### On Render.com (Cloud)
```
/Study-Hub/
├── Controllers/          ← Uses PrintQueueService
├── Service/
│   └── PrintQueueService.cs  ← Adds jobs to queue
├── Models/
│   └── Entities/
│       └── PrintJob.cs   ← Database model
└── Data/
    └── ApplicationDBContext.cs  ← Has PrintJobs DbSet
```

### On Your Local Machine
```
/Users/edward/Documents/StudyHubAPI/
├── diagnose-usb-printer-server.sh  ← Run this to check printer
└── LocalPrintServer/
    ├── Program.cs        ← Main print server app
    ├── appsettings.json  ← Database connection
    └── README.md         ← Detailed docs
```

---

## 🔄 Daily Operations

### Keep Local Print Server Running

**Development (while testing):**
```bash
cd LocalPrintServer
dotnet run
```

**Production (always running):**

**macOS:**
```bash
# Create launchd service (see LocalPrintServer/README.md)
# Server will auto-start on boot
```

**Linux:**
```bash
# Create systemd service (see LocalPrintServer/README.md)
sudo systemctl enable studyhub-print
sudo systemctl start studyhub-print
```

**Windows:**
```powershell
# Use NSSM or Task Scheduler (see LocalPrintServer/README.md)
```

---

## 🆘 Troubleshooting

### "No print jobs being processed"
```bash
# Check local print server is running
ps aux | grep dotnet

# Check local print server logs
# Should see: "Found 0 pending print jobs" every 5 seconds
```

### "Jobs stuck in Pending"
```bash
# Check local print server connection to database
# Check firewall isn't blocking connection
# Check database URL in appsettings.json is correct
```

### "Print fails with error"
```bash
# Run diagnostic on local machine
./diagnose-usb-printer-server.sh

# Check printer is connected and powered on
# Check USB cable
# Check permissions
```

### "Cannot connect to Render database"
```bash
# Make sure you're using INTERNAL Database URL, not External
# Render Dashboard → Database → Internal Database URL
# Should start with: postgresql://...render.com:5432/...
```

---

## 📚 Full Documentation

- **Quick Answer:** This file
- **Render.com Details:** `RENDER_DEPLOYMENT_PRINTING.md`
- **Local Print Server:** `LocalPrintServer/README.md`
- **USB Troubleshooting:** `USB_PRINTER_SERVER_DEPLOYMENT.md`
- **Technical Details:** `USB_PRINTER_FIX_SUMMARY.md`

---

## ✅ Success Checklist

- [ ] Backend deployed to Render.com
- [ ] PostgreSQL database created on Render.com
- [ ] PrintJob table migrated to database
- [ ] Local print server configured with database URL
- [ ] `diagnose-usb-printer-server.sh` shows printer OK
- [ ] Local print server running (dotnet run)
- [ ] Test print request creates job in database
- [ ] Local server processes job and prints
- [ ] Receipt prints on physical printer 🎉

---

## 🎉 You're Done!

Your setup:
- ✅ Backend running on Render.com (cloud)
- ✅ Database on Render.com PostgreSQL
- ✅ Local print server at your location
- ✅ USB printer connected locally
- ✅ Queue system connecting everything

**Print jobs flow automatically from cloud to your local printer!**

---

**Need help?** See the full documentation or run `./diagnose-usb-printer-server.sh`

