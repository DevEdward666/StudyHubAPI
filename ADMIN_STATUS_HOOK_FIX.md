# useAdminStatus Hook Fix - isAdmin Undefined Issue

## Problem
`isAdmin` was returning `undefined` instead of a boolean value.

## Root Cause
The hook was trying to access `query.data?.data`, but the `apiClient.get()` method already unwraps the `ApiResponse<T>` and returns `T` directly.

### How apiClient Works

**Backend Response:**
```json
{
  "success": true,
  "data": true,
  "message": null,
  "errors": null
}
```

**apiClient.get() Process:**
1. Receives response from backend
2. Validates with Zod schema: `ApiResponseSchema(z.boolean())`
3. Checks `success` field
4. **Returns `validatedResponse.data!` directly** (the boolean)
5. Does NOT return the full ApiResponse object

**Result:**
- `query.data` = `true` or `false` (boolean)
- NOT `{ success: true, data: true, ... }`

## Solution

### Before (Incorrect) ❌
```typescript
return {
  isAdmin: query.data?.data ?? false, // Trying to access .data on a boolean!
  // ...
};
```

### After (Correct) ✅
```typescript
return {
  isAdmin: query.data ?? false, // query.data is already the boolean value
  // ...
};
```

## Complete Fixed Hook

```typescript
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/api.client";
import { ApiResponseSchema } from "@/schema/api.schema";
import { z } from "zod";

export const useAdminStatus = () => {
  const query = useQuery({
    queryKey: ["adminStatus"],
    queryFn: () =>
      apiClient.get("/admin/is-admin", ApiResponseSchema(z.boolean())),
    retry: false,
    staleTime: 5 * 60 * 1000, // Consider fresh for 5 minutes
  });

  console.log("useAdminStatus query result:", query.data);

  return {
    isAdmin: query.data ?? false, // apiClient.get already unwraps and returns boolean
    isLoading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
  };
};
```

## Key Improvements

### 1. Fixed Data Access ✅
- Changed from `query.data?.data` to `query.data`
- Now correctly returns boolean value

### 2. Added Query Key ✅
- Added `queryKey: ["adminStatus"]`
- Enables proper caching and refetching

### 3. Added Stale Time ✅
- Added `staleTime: 5 * 60 * 1000` (5 minutes)
- Reduces unnecessary API calls
- Admin status doesn't change frequently

### 4. Added Logging ✅
- Added `console.log("useAdminStatus query result:", query.data)`
- Helps debug future issues

## Expected Behavior

### On Login (Admin User)
```typescript
// Console log:
useAdminStatus query result: true

// Hook returns:
{
  isAdmin: true,
  isLoading: false,
  error: null,
  refetch: [Function]
}
```

### On Login (Regular User)
```typescript
// Console log:
useAdminStatus query result: false

// Hook returns:
{
  isAdmin: false,
  isLoading: false,
  error: null,
  refetch: [Function]
}
```

### While Loading
```typescript
// Console log:
useAdminStatus query result: undefined

// Hook returns:
{
  isAdmin: false, // Fallback to false
  isLoading: true,
  error: null,
  refetch: [Function]
}
```

## Testing

### Verify Fix
```javascript
// In browser console after login
// Should log true (if admin) or false (if not admin)
// Should NOT log undefined
```

### Check Admin Status
```typescript
// In component using the hook
const { isAdmin, isLoading } = useAdminStatus();

console.log('Is Admin:', isAdmin); // Should be true or false
console.log('Is Loading:', isLoading); // Should be true or false
```

## Impact on Other Components

### TabsLayout.tsx ✅
```typescript
const { isAdmin, isLoading: isAdminLoading } = useAdminStatus();
// Now correctly receives boolean value
```

### UserManagement.tsx ✅
```typescript
const { isAdmin } = useAdminStatus();
// Now correctly receives boolean value
```

### NotificationProvider.tsx ✅
```typescript
const { isAdmin } = useAdminStatus();
// Now correctly receives boolean value
```

### AdminLogin.tsx ✅
```typescript
const { refetch: refetchAdminStatus } = useAdminStatus();
// Now correctly refetches admin status
```

## Files Modified

**File:** `/hooks/AdminHooks.tsx`

**Changes:**
1. ✅ Fixed data access: `query.data?.data` → `query.data`
2. ✅ Added query key for caching
3. ✅ Added stale time (5 minutes)
4. ✅ Added debug logging
5. ✅ Added helpful comments

## Verification Checklist

- [x] No TypeScript errors
- [x] Correct data access pattern
- [x] Query key added
- [x] Stale time configured
- [x] Logging added
- [x] Comments explain the logic

## Related Information

### apiClient.get() Method
```typescript
async get<T>(url: string, schema: z.ZodSchema<ApiResponse<T>>): Promise<T> {
  return this.request("GET", url, schema);
}
```

**Important:** Returns `T`, not `ApiResponse<T>`

### apiClient.request() Method
```typescript
async request<T>(...): Promise<T> {
  // ... validation ...
  return validatedResponse.data!; // ← Returns unwrapped data
}
```

**Important:** Unwraps the response and returns only the `data` field

---

**Status**: ✅ Fixed
**Issue**: `isAdmin` was undefined
**Cause**: Incorrect data access pattern (`query.data?.data` instead of `query.data`)
**Solution**: Use `query.data` directly as apiClient already unwraps the response
**Date**: November 22, 2025

