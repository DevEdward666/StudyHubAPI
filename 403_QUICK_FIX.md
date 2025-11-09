# ⚠️ 403 Forbidden Error - Quick Fix Guide

## The Error
```json
{
  "url": "/subscriptions/packages",
  "method": "POST",
  "status": 403,
  "message": "Request failed with status code 403"
}
```

## What This Means
**403 Forbidden** = You don't have permission to create subscription packages.

This endpoint is **ADMIN ONLY**.

---

## ✅ QUICK FIX

### Step 1: Are You Logged In as Admin?

**Check in browser console (F12):**
```javascript
const token = localStorage.getItem('auth_token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('Role:', payload.role);
}
```

**Expected:** `Role: Admin`

### Step 2: If Not Admin or Not Logged In

**Login as Admin:**
```
1. Logout (if logged in)
2. Go to /admin/login OR /login
3. Login with ADMIN credentials
4. System will redirect you to /app/admin/dashboard
5. Navigate to Subscription Packages via sidebar
```

### Step 3: Verify You Can See Admin Sidebar

After login, you should see:
- Hamburger menu (top left) with admin options
- Sidebar with: Dashboard, Tables, Users, etc.
- **Subscription Packages** option in sidebar

### Step 4: Try Creating Package Again

---

## Common Scenarios

### Scenario 1: "I'm not logged in"
**Fix:** Login via `/login` or `/admin/login`

### Scenario 2: "I'm logged in as regular user"
**Fix:** 
- Regular users CANNOT create packages
- Logout and login with admin account
- OR ask admin to promote your account

### Scenario 3: "I'm admin but still getting 403"
**Fix:**
- Token might be expired
- Logout and login again
- Clear browser cache: `localStorage.clear()`

### Scenario 4: "I don't have admin account"
**Fix:** Create admin user in database:
```sql
UPDATE users SET role = 'Admin' WHERE email = 'your-email@example.com';
```

---

## Test Your Auth Status

**Run in browser console:**
```javascript
// Check token
const token = localStorage.getItem('auth_token');
console.log('Has Token:', !!token);

// Check role
if (token) {
  try {
    const parts = token.split('.');
    const payload = JSON.parse(atob(parts[1]));
    console.log('User ID:', payload.nameid);
    console.log('Role:', payload.role);
    console.log('Is Admin:', payload.role === 'Admin');
    console.log('Expires:', new Date(payload.exp * 1000));
  } catch (e) {
    console.log('Invalid token');
  }
}
```

---

## Expected vs Actual

### ✅ WORKING (Admin User):
```
1. Login as admin
2. See admin sidebar
3. Click "Subscription Packages"
4. Click "Add New Package"
5. Fill form and submit
6. ✅ Success! Package created
```

### ❌ NOT WORKING (Regular User):
```
1. Login as regular user
2. See user tabs (bottom navigation)
3. Try to access admin features
4. ❌ 403 Forbidden error
```

---

## Who Can Create Subscription Packages?

**ONLY ADMINS** can:
- Create packages
- Edit packages
- Delete packages
- View all user subscriptions
- Purchase subscriptions for users

**REGULAR USERS** can:
- View available packages
- Purchase packages for themselves
- View their own subscriptions
- Track usage

---

## Immediate Action

**Right now, do this:**

1. **Check if logged in as admin:**
   ```javascript
   // Browser console (F12)
   const token = localStorage.getItem('auth_token');
   if (token) {
     const role = JSON.parse(atob(token.split('.')[1])).role;
     console.log('Your role:', role);
   }
   ```

2. **If not admin:**
   - Logout
   - Login with admin credentials
   - Verify you see admin sidebar

3. **If admin but still 403:**
   - Clear storage: `localStorage.clear()`
   - Login again
   - Try creating package

---

## Need Help?

**Check these:**
- [ ] Backend is running (`dotnet run`)
- [ ] You're logged in
- [ ] You're logged in as ADMIN (not regular user)
- [ ] Token is not expired
- [ ] You can access `/app/admin/dashboard`
- [ ] You can see admin sidebar

---

**Status:** ⚠️ Authorization Required  
**Who Can Fix:** Login as admin user  
**Quick Fix:** `/admin/login` → Login → Try again

