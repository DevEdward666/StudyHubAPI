# 🚨 BACKEND NOT RUNNING - START IT MANUALLY

## ⚠️ Current Situation

**Your backend is NOT running**, which is why you're getting the SignalR 404 error.

I tried to start it automatically but encountered port conflicts. You need to **start it manually**.

---

## ✅ SOLUTION: Start Backend Manually

### Step 1: Open a NEW Terminal Window

Don't use the current terminal - open a fresh one.

### Step 2: Navigate to Backend Directory

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
```

### Step 3: Start the Backend

```bash
dotnet run
```

### Step 4: Wait for Success Messages

You should see:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Study_Hub.Services.Background.SessionExpiryChecker[0]
      SessionExpiryChecker started. Checking every 1 minutes.
```

✅ **If you see "Application started"** - the backend is running!

---

## 🔴 If You Get "Address Already in Use" Error

This means something is already using the port. Here's how to fix it:

### Option 1: Find and Kill the Process

```bash
# Find what's using port 5212
lsof -i :5212

# You'll see something like:
# COMMAND   PID   USER
# dotnet    1234  edward

# Kill it (replace 1234 with the actual PID)
kill -9 1234

# Now try starting again
dotnet run
```

### Option 2: Use a Different Port

```bash
# Start on port 5214 instead
dotnet run --urls "http://localhost:5214;https://localhost:5215"
```

**⚠️ If you use a different port, you MUST update frontend .env:**

```bash
# Edit this file:
nano /Users/edward/Documents/StudyHubAPI/study_hub_app/.env.local

# Change this line:
VITE_API_URL=http://localhost:5214/api

# Save and restart frontend
```

### Option 3: Restart Your Mac

If all else fails:
1. Restart your Mac
2. Try `dotnet run` again

---

## 📋 Quick Troubleshooting Guide

### Problem: "dotnet: command not found"

**Solution:**
```bash
# Install .NET SDK
brew install dotnet-sdk

# Verify installation
dotnet --version
```

### Problem: Port 5212 Already in Use

**Solution:**
```bash
# Find the process
lsof -i :5212

# Kill it
kill -9 <PID>

# Or use different port
dotnet run --urls "http://localhost:5214"
```

### Problem: Database Connection Error

**Error:**
```
Failed to connect to database
```

**Solution:**
Check your connection string in `appsettings.json` - make sure the database is accessible.

### Problem: CORS Errors After Starting

**Solution:**
This was already fixed in the code. Just **restart the backend** after it starts the first time:

1. Stop backend (Ctrl+C)
2. Start again: `dotnet run`

---

## ✅ How to Verify Backend is Running

### Method 1: Check Logs

Look for this line:
```
Application started. Press Ctrl+C to shut down.
```

### Method 2: Test Health Endpoint

```bash
curl http://localhost:5000/health
```

Expected response:
```
Healthy
```

### Method 3: Open Swagger UI

Open in browser:
```
http://localhost:5000/swagger
```

You should see the API documentation.

### Method 4: Check SignalR Endpoint

```bash
curl -I http://localhost:5000/hubs/notifications
```

Should return `200 OK` or similar (NOT 404).

---

## 🎯 After Backend is Running

### Update Frontend .env (if needed)

If backend is running on **localhost:5000**:

```bash
# Edit .env file
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
nano .env.local

# Set this:
VITE_API_URL=http://localhost:5000/api
```

### Restart Frontend

```bash
# Stop frontend (Ctrl+C in its terminal)
# Start it again
npm run dev
```

### Check Browser Console

You should now see:

```
✅ SignalR connected successfully
✅ Transport used: WebSockets
✅ Joined admins group
```

**No more 404 errors!**

---

## 📝 Complete Startup Process

### Terminal 1: Backend

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
# Wait for "Application started"
# Leave this terminal running - DON'T close it
```

### Terminal 2: Frontend

```bash
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
# Wait for "Local: http://localhost:5173"
# Leave this terminal running - DON'T close it
```

### Browser

Open: `http://localhost:5173`

You should see:
- ✅ Frontend loads
- ✅ Can login
- ✅ No CORS errors
- ✅ SignalR connected
- ✅ App works!

---

## 🚀 Using DevTunnels (Optional)

If you want to access from phone/tablet:

### Start DevTunnel for Backend

```bash
# In a 3rd terminal
devtunnel host -p 5000 --allow-anonymous

# Note the URL, e.g.:
# https://abc123-5000.asse.devtunnels.ms
```

### Update Frontend .env

```bash
VITE_API_URL=https://abc123-5000.asse.devtunnels.ms/api
```

### Start DevTunnel for Frontend

```bash
# In a 4th terminal
devtunnel host -p 5173 --allow-anonymous

# Note the URL, e.g.:
# https://xyz789-5173.asse.devtunnels.ms
```

### Access from Phone

Open: `https://xyz789-5173.asse.devtunnels.ms`

---

## ⚡ Quick Start Script (Recommended)

Save this as `start-all.sh`:

```bash
#!/bin/bash

echo "🚀 Starting Study Hub..."

# Start backend in background
echo "📦 Starting backend..."
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run &
BACKEND_PID=$!

# Wait for backend
sleep 10

# Check if backend started
if curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo "✅ Backend running on http://localhost:5000"
else
    echo "❌ Backend failed to start"
    kill $BACKEND_PID
    exit 1
fi

# Start frontend
echo "🎨 Starting frontend..."
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
```

Make it executable:
```bash
chmod +x start-all.sh
```

Run it:
```bash
./start-all.sh
```

---

## ✅ Summary

**Current Status:**
- ❌ Backend is NOT running
- ❌ SignalR getting 404 errors
- ❌ Frontend can't connect

**What You Need to Do:**
1. **Open a new terminal**
2. **Run:** `cd /Users/edward/Documents/StudyHubAPI/Study-Hub && dotnet run`
3. **Wait for:** "Application started"
4. **Refresh your frontend**
5. **SignalR will connect** ✅

**The backend MUST be running for SignalR to work!**

Don't close the terminal where backend is running - keep it open!

---

**Next Step:** Open a terminal and run `dotnet run` in the Study-Hub folder NOW! 🚀

