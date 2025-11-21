# URL Construction Error Fix

## Error
```
TypeError: Failed to construct 'URL': Invalid base URL
```

## Root Cause
The `.env` file was empty, causing `import.meta.env.VITE_API_URL` to be `undefined`. When the SignalR service tried to construct a URL from `undefined`, it threw the error.

## Solution

### 1. **Fixed signalr.service.ts** ✅
Added proper URL validation and fallback:

```typescript
// Before (would crash if VITE_API_URL is undefined)
const apiBaseUrl = import.meta.env.VITE_API_URL || "https://...devtunnels.ms/api";
const baseUrl = apiBaseUrl.endsWith("/api")
  ? apiBaseUrl.substring(0, apiBaseUrl.length - 4)
  : apiBaseUrl.replace("/api/", "");

// After (validates and provides safe fallback)
const apiBaseUrl = import.meta.env.VITE_API_URL || "http://localhost:5212/api";

let baseUrl: string;
try {
  if (apiBaseUrl.endsWith("/api")) {
    baseUrl = apiBaseUrl.substring(0, apiBaseUrl.length - 4);
  } else if (apiBaseUrl.includes("/api/")) {
    baseUrl = apiBaseUrl.replace("/api/", "/");
  } else {
    baseUrl = apiBaseUrl;
  }
  
  new URL(baseUrl); // Validates URL
  console.log("SignalR base URL:", baseUrl);
} catch (error) {
  console.error("Invalid base URL, using default:", error);
  baseUrl = "http://localhost:5212";
}
```

### 2. **Updated api.client.ts** ✅
Changed default from devtunnel URL to localhost:

```typescript
// Before
baseURL: string = import.meta.env.VITE_API_BASE_URL || "https://...devtunnels.ms/api/"

// After
baseURL: string = import.meta.env.VITE_API_BASE_URL || "http://localhost:5212/api/"
```

### 3. **Created Environment Files** ✅

#### `.env.local` (for development)
```env
VITE_API_BASE_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api/
VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

#### `.env.example` (template)
```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5212/api/
VITE_API_URL=http://localhost:5212/api

# For production:
# VITE_API_BASE_URL=https://your-api.com/api/
# VITE_API_URL=https://your-api.com/api
```

## Files Modified

1. **`signalr.service.ts`**
   - Added URL validation with try-catch
   - Better error handling
   - Logging for debugging
   - Safe localhost fallback

2. **`api.client.ts`**
   - Updated default URL to localhost
   - Added console log for baseURL

3. **`.env.local`** (created)
   - Development environment variables

4. **`.env.example`** (created)
   - Template for configuration

## How to Use

### Development (DevTunnel Backend)
1. Copy `.env.example` to `.env.local`:
   ```bash
   cp .env.example .env.local
   ```

2. Keep default values (devtunnel):
   ```env
   VITE_API_BASE_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api/
   VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
   ```

3. Start frontend: `npm run dev`

### Development (Local Backend)
1. Update `.env.local`:
   ```env
   VITE_API_BASE_URL=http://localhost:5212/api/
   VITE_API_URL=http://localhost:5212/api
   ```

2. Start backend on port 5212
3. Start frontend: `npm run dev`

### Production (Deployed Backend)
1. Update `.env.local`:
   ```env
   VITE_API_BASE_URL=https://your-api.com/api/
   VITE_API_URL=https://your-api.com/api
   ```

2. Build and deploy:
   ```bash
   npm run build
   ```

## Verification

### Check Console Logs
After starting the app, you should see:
```
SignalR base URL: https://3qrbqpcx-5212.asse.devtunnels.ms
API Client initialized with baseURL: https://3qrbqpcx-5212.asse.devtunnels.ms/api/
```

### No More Errors
- ❌ Before: `TypeError: Failed to construct 'URL': Invalid base URL`
- ✅ After: URLs constructed successfully

## Common Issues

### Issue: Still Getting URL Error
**Solution:** 
1. Delete `.env` if it exists (it's empty anyway)
2. Ensure `.env.local` exists with correct values
3. Restart dev server: `npm run dev`

### Issue: Can't Connect to Backend
**Solution:**
1. Check backend is running on port 5212
2. Update `.env.local` with correct URL
3. Restart frontend

### Issue: Different Port Number
**Solution:**
Update `.env.local`:
```env
VITE_API_BASE_URL=http://localhost:YOUR_PORT/api/
VITE_API_URL=http://localhost:YOUR_PORT/api
```

## Testing

1. **Start backend:**
   ```bash
   cd Study-Hub
   dotnet run
   ```

2. **Start frontend:**
   ```bash
   cd study_hub_app
   npm run dev
   ```

3. **Check console:**
   - No URL construction errors
   - SignalR connects successfully
   - API requests work

## Notes

- `.env.local` is gitignored (won't be committed)
- `.env.example` should be committed (as template)
- Vite requires `VITE_` prefix for environment variables
- Changes to `.env.local` require dev server restart

---

**Status:** ✅ Fixed
**Date:** November 21, 2025

