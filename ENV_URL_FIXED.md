# ✅ FIXED: Wrong API URL in .env File

## 🐛 The Real Problem

The `.env` file had the **WRONG backend URL**!

### ❌ Before (WRONG):
```env
VITE_API_URL=https://3qrbqpcx-5173.asse.devtunnels.ms/api
```

This was pointing to **port 5173** which is the **FRONTEND** port, not the backend!

### ✅ After (CORRECT):
```env
VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

Now pointing to **port 5212** which is the **BACKEND** port!

---

## 🔍 What Was Happening

1. **Frontend** was trying to connect to SignalR at:
   ```
   https://3qrbqpcx-5173.asse.devtunnels.ms/hubs/notifications
   ```

2. **But port 5173 is the FRONTEND**, which doesn't have a SignalR hub!

3. **Result:** 404 error - "hubs/notifications/ is missing"

4. **Backend** is actually running on:
   ```
   https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
   ```

---

## ✅ The Fix Applied

I changed the `.env` file to point to the correct backend URL:

**File:** `/Users/edward/Documents/StudyHubAPI/study_hub_app/.env`

```diff
- VITE_API_URL=https://3qrbqpcx-5173.asse.devtunnels.ms/api
+ VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

---

## 🚀 What You Need to Do Now

### Step 1: Make Sure Backend is Running

Open a terminal and start the backend:

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

Wait for:
```
Application started. Press Ctrl+C to shut down.
```

### Step 2: Restart the Frontend

Since we changed the `.env` file, the frontend needs to restart:

1. **Stop the frontend** (Ctrl+C in its terminal)

2. **Start it again:**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/study_hub_app
   npm run dev
   ```

3. **Refresh your browser**

### Step 3: Verify SignalR Connection

Check browser console - you should now see:

```
✅ Creating SignalR connection to: https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
✅ SignalR connected successfully
✅ Transport used: WebSockets
✅ Joined admins group
```

**No more 404 errors!** ✅

---

## 📊 Port Reference

To avoid confusion in the future:

| Service | Port | DevTunnel URL | Purpose |
|---------|------|---------------|---------|
| **Backend** | 5212 | `https://3qrbqpcx-5212.asse.devtunnels.ms` | API + SignalR |
| **Frontend** | 5173 | `https://3qrbqpcx-5173.asse.devtunnels.ms` | React App |

### Important URLs:

**Backend (5212):**
- API Base: `https://3qrbqpcx-5212.asse.devtunnels.ms/api`
- SignalR Hub: `https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications`
- Swagger: `https://3qrbqpcx-5212.asse.devtunnels.ms/swagger`
- Health: `https://3qrbqpcx-5212.asse.devtunnels.ms/health`

**Frontend (5173):**
- App: `https://3qrbqpcx-5173.asse.devtunnels.ms`

---

## 🔧 How .env Works

The `.env` file is read when the frontend **starts**:

```typescript
// In signalr.service.ts
const apiBaseUrl = import.meta.env.VITE_API_URL || "fallback-url";
```

**This means:**
- ✅ Changes to `.env` require **frontend restart**
- ✅ `VITE_API_URL` should point to **backend** (5212)
- ❌ Don't point it to frontend (5173)

---

## 🎯 Testing the Fix

### Test 1: Check Backend is Running

```bash
# Should return "Healthy"
curl https://3qrbqpcx-5212.asse.devtunnels.ms/health
```

### Test 2: Check SignalR Endpoint Exists

```bash
# Should NOT return 404
curl -I https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
```

### Test 3: Check Frontend Connects

Open browser console and look for:
```
✅ SignalR connected successfully
```

### Test 4: Test API Calls

Try logging in - you should see:
```
POST https://3qrbqpcx-5212.asse.devtunnels.ms/api/auth/signin
Status: 200 OK
```

---

## 📝 Complete Startup Checklist

### ✅ Backend Running:
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run

# Expected output:
# ✅ Now listening on: http://localhost:5212
# ✅ Application started
# ✅ SessionExpiryChecker started
```

### ✅ Frontend .env Correct:
```env
VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

### ✅ Frontend Running:
```bash
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev

# Expected output:
# ✅ Local: http://localhost:5173
# ✅ Network: use --host to expose
```

### ✅ Browser Console:
```
✅ SignalR connected successfully
✅ No 404 errors
✅ No CORS errors
```

---

## 🚨 Common Mistakes to Avoid

### Mistake 1: Using Frontend URL for API

```env
❌ WRONG: VITE_API_URL=https://3qrbqpcx-5173.asse.devtunnels.ms/api
✅ RIGHT: VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

### Mistake 2: Forgetting to Restart Frontend

After changing `.env`, you **MUST** restart the frontend:
```bash
# Stop with Ctrl+C
npm run dev  # Start again
```

### Mistake 3: Using Localhost Instead of DevTunnel

If accessing from phone/tablet, use DevTunnel URLs, not localhost:

```env
❌ WRONG for mobile: VITE_API_URL=http://localhost:5212/api
✅ RIGHT for mobile: VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

---

## 🎉 Summary

### What Was Wrong:
- ❌ `.env` pointed to frontend URL (5173) instead of backend (5212)
- ❌ SignalR tried to connect to frontend
- ❌ Got 404 because frontend doesn't have `/hubs/notifications`

### What I Fixed:
- ✅ Changed `.env` to point to correct backend URL (5212)
- ✅ Also fixed CORS configuration earlier
- ✅ Also fixed SessionExpiryChecker earlier

### What You Need to Do:
1. ✅ **Start backend:** `dotnet run` in Study-Hub folder
2. ✅ **Restart frontend:** Stop and `npm run dev` again
3. ✅ **Refresh browser**
4. ✅ **Verify:** SignalR connects successfully

---

**All fixes are now in place! Just start the backend and restart the frontend!** 🚀

---

**Date:** November 8, 2025  
**Issue:** Wrong API URL in .env - pointed to frontend instead of backend  
**File Fixed:** `/Users/edward/Documents/StudyHubAPI/study_hub_app/.env`  
**Change:** Port 5173 → 5212  
**Status:** ✅ FIXED - Restart frontend required

