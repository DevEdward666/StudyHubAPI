# ✅ SIGNALR 404 ERROR - BACKEND NOT RUNNING

## 🐛 Error Message

```
Failed to start the connection: Error: Failed to complete negotiation with the server: 
Error: Status code '404' 
Either this is not a SignalR endpoint or there is a proxy blocking the connection.
```

## 🔍 Root Cause

**The backend server is NOT running!**

The 404 error means:
- ❌ Backend server is stopped
- ❌ SignalR hub endpoint doesn't exist (because server isn't running)
- ❌ Frontend can't connect to `https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications`

**This is NOT a code error** - the backend is properly configured, it's just not running.

---

## ✅ Solution: Start the Backend Server

### Step 1: Open a Terminal

```bash
# Navigate to backend directory
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
```

### Step 2: Start the Backend

```bash
# Start the backend server
dotnet run
```

### Step 3: Wait for Startup Messages

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5212
      Now listening on: https://localhost:5213
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: SessionExpiryChecker[0]
      SessionExpiryChecker started. Checking every 1 minutes.
```

### Step 4: Verify SignalR Hub is Running

Once backend is running, check in logs for:
```
info: Microsoft.AspNetCore.SignalR.HubConnectionContext[0]
      SignalR Hub configured
```

---

## 🔧 Using DevTunnels (For Remote Access)

If you're using DevTunnels, you also need to start the tunnel:

### Option 1: VS Code Dev Tunnels

1. **Open VS Code**
2. **Open Command Palette** (Cmd+Shift+P on Mac)
3. **Type:** "Dev Tunnels: Start Tunnel"
4. **Select your backend project**
5. **Note the tunnel URL** (e.g., `https://3qrbqpcx-5212.asse.devtunnels.ms`)

### Option 2: Command Line Dev Tunnels

```bash
# Install devtunnel CLI if not installed
brew install devtunnel

# Start tunnel
devtunnel host -p 5212 --allow-anonymous
```

---

## 📋 Complete Startup Checklist

### ✅ Backend Startup Process:

1. **Navigate to backend folder:**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/Study-Hub
   ```

2. **Start the backend:**
   ```bash
   dotnet run
   ```

3. **Check for startup logs:**
   ```
   ✅ "Now listening on: http://localhost:5212"
   ✅ "Application started"
   ✅ "SessionExpiryChecker started"
   ```

4. **Verify endpoints:**
   - API: `http://localhost:5212/api`
   - SignalR: `http://localhost:5212/hubs/notifications`
   - Health: `http://localhost:5212/health`
   - Swagger: `http://localhost:5212/swagger`

5. **If using DevTunnels, start tunnel:**
   ```bash
   # Via VS Code or CLI
   devtunnel host -p 5212 --allow-anonymous
   ```

6. **Check tunnel is active:**
   ```
   ✅ Tunnel URL: https://3qrbqpcx-5212.asse.devtunnels.ms
   ✅ Can access: https://3qrbqpcx-5212.asse.devtunnels.ms/health
   ```

### ✅ Frontend Startup Process:

1. **Navigate to frontend folder:**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/study_hub_app
   ```

2. **Start the frontend:**
   ```bash
   npm run dev
   ```

3. **If using DevTunnels for frontend:**
   ```bash
   # Start frontend tunnel on port 5173
   devtunnel host -p 5173 --allow-anonymous
   ```

4. **Access frontend:**
   - Local: `http://localhost:5173`
   - Tunnel: `https://3qrbqpcx-5173.asse.devtunnels.ms`

---

## 🎯 What Should Happen After Starting Backend

### 1. Backend Logs Show:

```
[2025-11-08 14:30:00] info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5212
[2025-11-08 14:30:00] info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5213
[2025-11-08 14:30:00] info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
[2025-11-08 14:30:00] info: Study_Hub.Services.Background.SessionExpiryChecker[0]
      SessionExpiryChecker started. Checking every 1 minutes.
```

### 2. Frontend SignalR Connects:

In browser console:
```
✅ Creating SignalR connection to: https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
✅ SignalR connected successfully
✅ Transport used: WebSockets
✅ Joined admins group
```

### 3. No More 404 Errors:

Instead of:
```
❌ Status code '404'
```

You'll see:
```
✅ SignalR connected successfully
```

---

## 🚨 Common Issues and Solutions

### Issue 1: "dotnet: command not found"

**Solution:**
```bash
# Install .NET SDK
brew install dotnet-sdk
```

### Issue 2: Port Already in Use

**Error:**
```
Failed to bind to address http://localhost:5212: address already in use
```

**Solution:**
```bash
# Find process using port 5212
lsof -i :5212

# Kill the process
kill -9 <PID>

# Or use a different port
dotnet run --urls "http://localhost:5214"
```

### Issue 3: CORS Errors After Starting Backend

**Verify CORS is configured correctly:**

Backend logs should show:
```
info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[0]
      CORS policy evaluation succeeded.
```

If you see CORS errors, **restart the backend** (we fixed CORS configuration earlier).

### Issue 4: DevTunnel URL Changed

**Problem:** DevTunnel URLs change each time you restart

**Solution:**
1. Note the new tunnel URL
2. Update frontend `.env` file:
   ```bash
   VITE_API_URL=https://NEW-TUNNEL-URL.asse.devtunnels.ms/api
   ```
3. Restart frontend

---

## 🔍 How to Check if Backend is Running

### Method 1: Check Process

```bash
# Check if dotnet process is running
ps aux | grep dotnet
```

### Method 2: Test Health Endpoint

```bash
# Local
curl http://localhost:5212/health

# DevTunnel
curl https://3qrbqpcx-5212.asse.devtunnels.ms/health
```

**Expected Response:**
```
Healthy
```

### Method 3: Test SignalR Endpoint

```bash
# This should return 200 or redirect, NOT 404
curl -I http://localhost:5212/hubs/notifications
```

**Expected Response:**
```
HTTP/1.1 200 OK
(or some other non-404 response)
```

### Method 4: Check Swagger UI

Open in browser:
- Local: `http://localhost:5212/swagger`
- DevTunnel: `https://3qrbqpcx-5212.asse.devtunnels.ms/swagger`

**Expected:** Swagger documentation page loads ✅

---

## 📝 Backend Startup Script (Optional)

Create a script to start both backend and tunnels:

**File:** `/Users/edward/Documents/StudyHubAPI/start-backend.sh`

```bash
#!/bin/bash

echo "🚀 Starting Study Hub Backend..."

# Navigate to backend
cd /Users/edward/Documents/StudyHubAPI/Study-Hub

# Start backend
echo "📦 Starting .NET backend..."
dotnet run &

# Wait for backend to start
echo "⏳ Waiting for backend to initialize..."
sleep 5

# Check if backend is running
if curl -s http://localhost:5212/health > /dev/null; then
    echo "✅ Backend is running!"
    echo "📍 Local: http://localhost:5212"
    echo "📍 Swagger: http://localhost:5212/swagger"
    echo "📍 SignalR: http://localhost:5212/hubs/notifications"
else
    echo "❌ Backend failed to start"
    exit 1
fi

echo ""
echo "🎉 Backend ready!"
echo "Press Ctrl+C to stop"

# Keep script running
wait
```

**Make it executable:**
```bash
chmod +x /Users/edward/Documents/StudyHubAPI/start-backend.sh
```

**Run it:**
```bash
./start-backend.sh
```

---

## 🎯 Quick Fix Steps

**If you just want to fix the 404 error NOW:**

1. **Open Terminal**
2. **Run these commands:**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/Study-Hub
   dotnet run
   ```
3. **Wait for:** "Application started"
4. **Refresh your frontend**
5. **SignalR should connect** ✅

---

## ✅ After Backend is Running

You should see in **frontend console**:

```
✅ Creating SignalR connection to: https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
✅ SignalR connected successfully
✅ Transport used: WebSockets
✅ Joined admins group
```

And in **backend logs**:

```
[INF] SignalR connection established
[INF] User joined admins group
```

**No more 404 errors!** ✅

---

## 📊 Summary

| Issue | Cause | Solution |
|-------|-------|----------|
| SignalR 404 | Backend not running | Start backend with `dotnet run` |
| Connection timeout | Backend not accessible | Check backend logs, verify tunnel |
| CORS errors | Old CORS config | Restart backend (we fixed CORS) |
| Wrong endpoint | DevTunnel URL changed | Update `.env` with new tunnel URL |

---

## 🚀 Final Checklist

Before trying to use the app:

- [ ] Backend server is running (`dotnet run`)
- [ ] Backend shows "Application started" in logs
- [ ] Health endpoint responds: `curl http://localhost:5212/health`
- [ ] SignalR endpoint exists: `curl -I http://localhost:5212/hubs/notifications`
- [ ] If using DevTunnels, tunnel is active
- [ ] Frontend is running (`npm run dev`)
- [ ] Frontend `.env` has correct backend URL
- [ ] No 404 errors in browser console
- [ ] SignalR shows "connected successfully"

---

**Date:** November 8, 2025  
**Error:** SignalR 404 - Failed to complete negotiation  
**Cause:** Backend server not running  
**Resolution:** Start backend with `dotnet run`  
**Status:** ⚠️ **START THE BACKEND SERVER NOW**

