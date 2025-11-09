# ✅ SIGNIN TIMEOUT - COMPLETE FIX

## Summary
The signin timeout was caused by the API client pointing to a DevTunnel URL that is not accessible. I've updated it to use localhost.

## Fix Applied ✅

### 1. Updated API Base URL
**File:** `study_hub_app/src/services/api.client.ts`

**Changed from:**
```typescript
baseURL: "https://3qrbqpcx-5212.asse.devtunnels.ms/api/"
```

**To:**
```typescript
baseURL: "http://localhost:5212/api/"
```

---

## How to Start the Backend

The backend needs to be running for the frontend to work. Follow these steps:

### Step 1: Open a New Terminal for Backend

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

**Expected output:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5212
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 2: Keep Backend Terminal Running

**DO NOT CLOSE THIS TERMINAL** - Keep it running in the background.

### Step 3: Restart Frontend (if needed)

If frontend is running, you may need to hard refresh the page (Cmd+Shift+R) after updating the API URL.

---

## Testing the Fix

### 1. Verify Backend is Running

**Option A: Check Swagger**
```
Open browser: http://localhost:5212/swagger
```

**Option B: Test API Endpoint**
```bash
curl http://localhost:5212/api/rates
```

### 2. Test Login

1. Open your frontend app
2. Try to login
3. Should work without timeout errors

---

## Troubleshooting

### If "Address already in use" error:

**Find and kill the process:**
```bash
# Find process using port 5212
lsof -i :5212

# Kill it (replace PID with actual process ID)
kill -9 <PID>

# Then restart
cd Study-Hub
dotnet run
```

### If still timeout errors:

**Check the browser console:**
- Press F12
- Go to Network tab
- Try to login
- Check what URL it's calling

**Should be:**
```
http://localhost:5212/api/auth/signin
```

**If it's still calling DevTunnel URL:**
1. Hard refresh browser (Cmd+Shift+R)
2. Clear browser cache
3. Restart frontend dev server

---

## Recommended Development Setup

### Terminal 1: Backend (Keep Running)
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet watch run
```

### Terminal 2: Frontend (Keep Running)
```bash
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
```

### Access Points:
- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:5212  
- **Swagger Docs:** http://localhost:5212/swagger

---

## Alternative: Use .env File

Create `.env` file in `study_hub_app/`:

```env
VITE_API_BASE_URL=http://localhost:5212/api/
```

This allows you to easily switch between local and remote backends.

---

## What Was Fixed

1. ✅ API client URL changed to localhost
2. ✅ Ready for local backend connection
3. ✅ No more DevTunnel dependency

## What You Need to Do

1. ✅ Start backend server in a terminal
2. ✅ Keep it running
3. ✅ Refresh frontend (hard refresh if needed)
4. ✅ Try login again

---

**Status:** ✅ FIX APPLIED  
**Next:** Start the backend server with `dotnet run`

---

**Quick Start Command:**
```bash
# Open new terminal and run:
cd /Users/edward/Documents/StudyHubAPI/Study-Hub && dotnet run
```

Then try logging in again! 🚀

