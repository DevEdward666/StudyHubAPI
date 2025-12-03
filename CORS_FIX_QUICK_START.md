# ⚡ CORS FIX - QUICK START GUIDE

## 🔴 THE PROBLEM
```
Access to XMLHttpRequest blocked by CORS policy
Origin: http://localhost:5173
Target: https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

## ✅ THE FIX (ALREADY APPLIED)
Updated `Program.cs` to allow `http://localhost:5173` explicitly.

## 🚀 RESTART BACKEND NOW

### Method 1: Use the Auto-Restart Script (RECOMMENDED)
```bash
cd /Users/edward/Documents/StudyHubAPI
./restart-backend-with-cors-fix.sh
```

### Method 2: Manual Restart
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub

# Build
dotnet build

# Run
dotnet run
```

### Method 3: If Using DevTunnel
```bash
# In one terminal:
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run

# In another terminal:
devtunnel host -p 5212
```

## 🧪 TEST IT

1. **Start backend** (using method above)
2. **Open frontend** at http://localhost:5173
3. **Try to login** - CORS error should be GONE! ✅
4. **Check backend logs** - you should see:
   ```
   🔍 CORS Request from origin: http://localhost:5173
   ✅ CORS: Allowed localhost origin: http://localhost:5173
   ```

## 📊 VERIFICATION

### Backend Logs Should Show:
```
🔍 CORS Request from origin: http://localhost:5173
✅ CORS: Allowed localhost origin: http://localhost:5173
```

### Browser Network Tab Should Show:
- ✅ OPTIONS request succeeds (status 200 or 204)
- ✅ POST request succeeds
- ✅ No CORS error in console

### Response Headers Should Include:
```
Access-Control-Allow-Origin: http://localhost:5173
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
```

## ❌ STILL GETTING CORS ERROR?

### 1. Backend not restarted
- **Solution**: Restart using commands above

### 2. Browser cache
- **Solution**: Hard refresh (Cmd+Shift+R) or use Incognito

### 3. Wrong backend URL
- **Check**: `study_hub_app/.env`
- **Should be**: `VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api`

### 4. Frontend not restarted
```bash
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
# Ctrl+C to stop
npm run dev
```

## 📝 WHAT WAS CHANGED

File: `Study-Hub/Program.cs`

Added explicit localhost support:
```csharp
if (origin.StartsWith("http://localhost") 
    || origin.StartsWith("https://localhost")
    || origin.StartsWith("http://127.0.0.1")
    || origin.StartsWith("https://127.0.0.1"))
{
    Console.WriteLine($"✅ CORS: Allowed localhost origin: {origin}");
    return true;
}
```

## 🎯 NEXT STEPS

1. ✅ Code updated (DONE)
2. 🔄 Restart backend (DO THIS NOW!)
3. 🧪 Test login (verify it works)
4. 🎉 Continue development

---
**Status**: ✅ Code ready | ⏳ Awaiting backend restart
**Date**: December 3, 2025

