# ✅ Subscription Package 403 Forbidden Error Fix

## Error Details
```json
{
  "url": "/subscriptions/packages",
  "method": "POST",
  "status": 403,
  "data": "",
  "message": "Request failed with status code 403"
}
```

## Problem
**HTTP 403 Forbidden** means the server is refusing to authorize the request. This typically happens because:

1. ❌ User is not logged in
2. ❌ User is not an admin
3. ❌ Auth token is missing or expired
4. ❌ Auth token is not being sent with the request

## Quick Diagnosis

### Check 1: Are You Logged In?
- Open browser console (F12)
- Run: `localStorage.getItem('auth_token')`
- **Should show:** A token string
- **If null:** You need to login

### Check 2: Are You Logged In as Admin?
- The subscription package management is admin-only
- Regular users will get 403 error
- You must login with an admin account

### Check 3: Is Token Being Sent?
- Open Network tab in browser console
- Try to create a package
- Click on the request
- Check Headers
- Look for: `Authorization: Bearer {token}`
- **If missing:** API client is not sending the token

---

## Solutions

### Solution 1: Login as Admin ✅

**Step 1: Logout if logged in**
```
Click Profile → Logout
```

**Step 2: Go to Admin Login**
```
Navigate to: /admin/login
```

**Step 3: Login with Admin Credentials**
- Use admin email and password
- System should redirect to admin dashboard

**Step 4: Navigate to Subscription Packages**
```
Sidebar → Subscription Packages
```

---

### Solution 2: Check Auth Token Storage

**Open Browser Console (F12) and run:**
```javascript
// Check if token exists
const token = localStorage.getItem('auth_token');
console.log('Token:', token);

// If token exists, decode it to check expiry
if (token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    console.log('Token Payload:', payload);
    console.log('Expires:', new Date(payload.exp * 1000));
    console.log('Is Admin:', payload.role === 'Admin');
  } catch (e) {
    console.log('Invalid token format');
  }
}
```

**Expected output:**
```
Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Token Payload: { nameid: "...", role: "Admin", nbf: ..., exp: ..., iat: ... }
Expires: Fri Nov 08 2025 23:59:59
Is Admin: true
```

**If token is expired or missing:**
- Clear storage: `localStorage.clear()`
- Login again

---

### Solution 3: Verify API Client is Sending Token

The API client should automatically attach the token. Let's verify:

**File:** `src/services/api.client.ts`

Check this code exists:
```typescript
private setupInterceptors() {
  // Request interceptor to add auth token
  this.client.interceptors.request.use((config) => {
    const token = this.getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
}

private getAuthToken(): string | null {
  return localStorage.getItem("auth_token");
}
```

---

### Solution 4: Backend Authorization Check

**Verify backend endpoint has [Authorize] attribute:**

File: `SubscriptionsController.cs`

```csharp
[Authorize] // ✅ Must have this
[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    [HttpPost("packages")]
    [Authorize(Roles = "Admin")] // ✅ Admin only
    public async Task<ActionResult<ApiResponse<SubscriptionPackageDto>>> CreatePackage(
        CreateSubscriptionPackageDto request)
    {
        // ...
    }
}
```

---

## Step-by-Step Fix

### For Regular Users:
**You cannot create subscription packages as a regular user.** Only admins can manage packages.

**To purchase a subscription:**
1. Login as regular user
2. Go to "Subscriptions" tab (bottom navigation)
3. Click "Buy New Subscription"
4. Select from available packages (created by admin)

### For Admins:

**Step 1: Ensure You're Logged In as Admin**
```
1. Logout if logged in
2. Go to /admin/login
3. Login with admin credentials
4. Verify you see admin sidebar
```

**Step 2: Verify Token**
```javascript
// In browser console
localStorage.getItem('auth_token')
// Should return a token
```

**Step 3: Navigate to Subscription Packages**
```
Admin Sidebar → Subscription Packages
```

**Step 4: Create Package**
```
Click "Add New Package"
Fill in the form
Click "Create Package"
```

---

## Common Issues & Solutions

### Issue 1: "Not logged in"
**Solution:**
```
1. Go to /admin/login
2. Login with admin account
3. Try again
```

### Issue 2: "Logged in but not as admin"
**Solution:**
```
1. Logout
2. Login with admin account (not regular user)
3. Admin login is at: /admin/login
```

### Issue 3: "Token expired"
**Solution:**
```javascript
// Clear expired token
localStorage.clear();
// Login again
```

### Issue 4: "Token not being sent"
**Check Network tab:**
- Request should have `Authorization: Bearer {token}` header
- If missing, check api.client.ts interceptors

---

## Testing Authorization

### Test 1: Check if logged in as admin
```javascript
// Browser console
const token = localStorage.getItem('auth_token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('Role:', payload.role);
  console.log('Is Admin:', payload.role === 'Admin');
}
```

### Test 2: Test API with current token
```javascript
// Browser console
fetch('http://localhost:5212/api/subscriptions/packages', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
  }
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

**Expected:**
- 200 OK with packages list (if admin)
- 403 Forbidden (if not admin or not logged in)
- 401 Unauthorized (if token invalid/expired)

---

## Quick Fix Checklist

- [ ] Logged in as admin (not regular user)
- [ ] Token exists in localStorage
- [ ] Token not expired
- [ ] Using /admin/login (not /login)
- [ ] Can see admin sidebar
- [ ] Backend is running
- [ ] API returns 200 for GET /subscriptions/packages

---

## Admin Login Credentials

**If you don't have admin credentials, create one:**

### Method 1: Via Database
```sql
-- Update existing user to admin
UPDATE users 
SET role = 'Admin' 
WHERE email = 'your-email@example.com';
```

### Method 2: Via Backend Endpoint (if available)
```
POST /api/admin/users/toggle-admin
{
  "userId": "user-guid-here"
}
```

### Method 3: Register and Promote
```
1. Register a new account
2. Use database/backend to promote to admin
3. Login with that account
```

---

## Expected Flow

### ✅ Correct Flow:
```
1. Navigate to /admin/login
2. Login with admin credentials
3. System stores auth_token in localStorage
4. System redirects to admin dashboard
5. Click Sidebar → Subscription Packages
6. Click "Add New Package"
7. Fill form and submit
8. Request sent with Authorization header
9. Backend validates admin role
10. Package created successfully
```

### ❌ Why 403 Happens:
```
1. Not logged in → No token → 403
2. Logged in as user → Not admin → 403
3. Token expired → Invalid auth → 403
4. Token not sent → No Authorization header → 403
```

---

## Status Codes Reference

- **200 OK** - Success, you're authorized
- **401 Unauthorized** - Token missing, invalid, or expired
- **403 Forbidden** - You don't have admin permissions
- **404 Not Found** - Endpoint doesn't exist

---

## Immediate Action

**Run this in browser console:**
```javascript
console.log('Auth Token:', localStorage.getItem('auth_token'));
```

**If null or undefined:**
```
→ Go to /admin/login and login
```

**If exists:**
```javascript
// Check if admin
const payload = JSON.parse(atob(localStorage.getItem('auth_token').split('.')[1]));
console.log('Role:', payload.role);
```

**If role is not "Admin":**
```
→ Logout and login with admin account
```

**If role is "Admin":**
```
→ Token might be expired, login again
```

---

**Status:** ⚠️ AUTHORIZATION ISSUE  
**Quick Fix:** Login as admin via /admin/login  
**Date:** November 8, 2025

