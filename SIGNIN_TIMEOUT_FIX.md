# ⚠️ Signin Timeout Error - Troubleshooting Guide

## Error Details
```json
{
  "url": "/auth/signin",
  "method": "POST",
  "message": "timeout of 10000ms exceeded"
}
```

## What This Means
The frontend is trying to connect to the backend API at `/auth/signin` but the request is timing out after 10 seconds. This indicates the backend server is either:
- Not running
- Running on a different port
- Not accessible from the frontend

---

## Quick Fixes

### 1. ✅ Check if Backend is Running

**Open a new terminal and run:**
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5212
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 2. ✅ Verify API Base URL

**Check your frontend API client configuration:**

File: `/Users/edward/Documents/StudyHubAPI/study_hub_app/src/services/api.client.ts`

Current base URL:
```typescript
baseURL: import.meta.env.VITE_API_BASE_URL || "https://3qrbqpcx-5212.asse.devtunnels.ms/api/"
```

**For local development, update to:**
```typescript
baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5212/api/"
```

### 3. ✅ Create/Update .env File

**Create `.env` file in `/study_hub_app/` directory:**
```env
VITE_API_BASE_URL=http://localhost:5212/api/
```

### 4. ✅ Restart Frontend Dev Server

After making changes:
```bash
# Stop the current dev server (Ctrl+C)
# Then restart:
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
```

---

## Step-by-Step Resolution

### Step 1: Start Backend Server

```bash
# Terminal 1: Backend
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

**Wait for:**
```
Now listening on: http://localhost:5212
```

### Step 2: Verify Backend is Accessible

**Open browser and visit:**
```
http://localhost:5212/swagger
```

You should see the Swagger API documentation page.

### Step 3: Update Frontend API URL (if using DevTunnel)

**If you're using DevTunnel URL `https://3qrbqpcx-5212.asse.devtunnels.ms/api/`:**

Make sure:
1. DevTunnel is running and active
2. The URL is still valid (DevTunnel URLs can expire)
3. You have internet connection

**To use local backend instead, update `api.client.ts`:**

```typescript
// Change this line:
baseURL: import.meta.env.VITE_API_BASE_URL || "https://3qrbqpcx-5212.asse.devtunnels.ms/api/"

// To this (for local development):
baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5212/api/"
```

### Step 4: Test the Connection

1. Open browser console (F12)
2. Try to login
3. Check Network tab for the request details

---

## Common Issues & Solutions

### Issue 1: Backend Not Running
**Solution:** Start the backend server
```bash
cd Study-Hub
dotnet run
```

### Issue 2: Wrong Port
**Solution:** Check what port backend is running on
```bash
# Backend should show:
Now listening on: http://localhost:5212
```

### Issue 3: CORS Error
**Solution:** Backend should already have CORS configured. If you see CORS errors:

Check `Program.cs` has:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

### Issue 4: DevTunnel URL Expired
**Solution:** Use local backend or create new DevTunnel

**For local:**
```typescript
// api.client.ts
baseURL: "http://localhost:5212/api/"
```

**For new DevTunnel:**
```bash
devtunnel create
devtunnel port create -p 5212
devtunnel host
```

---

## Quick Test Commands

### Test Backend is Running:
```bash
curl http://localhost:5212/api/rates
```

Should return JSON response (or 401 Unauthorized if auth is required).

### Test Frontend Can Reach Backend:
Open browser console and run:
```javascript
fetch('http://localhost:5212/api/rates')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error)
```

---

## Recommended Setup for Development

### Terminal 1: Backend
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet watch run
```

### Terminal 2: Frontend
```bash
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
```

### Access Points:
- **Frontend:** http://localhost:5173 (or whatever Vite shows)
- **Backend API:** http://localhost:5212
- **Swagger Docs:** http://localhost:5212/swagger

---

## Network Configuration Check

### If using local network (both on localhost):
```env
# .env in study_hub_app/
VITE_API_BASE_URL=http://localhost:5212/api/
```

### If backend is on remote server:
```env
# .env in study_hub_app/
VITE_API_BASE_URL=https://your-backend-url.com/api/
```

---

## Immediate Action Items

1. ✅ **Start Backend Server**
   ```bash
   cd Study-Hub && dotnet run
   ```

2. ✅ **Verify Backend Endpoint**
   ```
   Visit: http://localhost:5212/swagger
   ```

3. ✅ **Update API URL if needed**
   ```typescript
   // api.client.ts - use localhost for local dev
   baseURL: "http://localhost:5212/api/"
   ```

4. ✅ **Restart Frontend**
   ```bash
   # Stop (Ctrl+C) and restart
   npm run dev
   ```

5. ✅ **Try Login Again**

---

## Still Not Working?

### Check These:

1. **Backend Port:**
   ```bash
   lsof -i :5212
   # Should show dotnet process
   ```

2. **Frontend Port:**
   ```bash
   lsof -i :5173
   # Should show node/vite process
   ```

3. **Firewall:**
   - Make sure port 5212 is not blocked
   - Check macOS firewall settings

4. **Environment Variables:**
   ```bash
   # In study_hub_app directory
   cat .env
   # Should show VITE_API_BASE_URL
   ```

---

## Success Indicators

✅ Backend running: See "Now listening on: http://localhost:5212"
✅ Swagger accessible: http://localhost:5212/swagger loads
✅ Frontend running: Vite shows local URL
✅ No timeout errors in console
✅ Login works successfully

---

**Next Step:** Start the backend server and verify it's running on port 5212

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

Then try logging in again!

---

**Date:** November 8, 2025  
**Issue:** Signin timeout  
**Cause:** Backend not running or URL mismatch  
**Solution:** Start backend and verify API URL configuration

