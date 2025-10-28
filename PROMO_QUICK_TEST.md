# 🚀 Promo System - Quick Start Guide

## ✅ What's Ready

- ✅ Backend API (14 endpoints)
- ✅ Database migration applied
- ✅ Frontend UI complete
- ✅ Full integration

---

## 🎯 Quick Test (5 Minutes)

### 1. Start Backend
```bash
cd Study-Hub
dotnet run
```

### 2. Start Frontend
```bash
cd study_hub_app
npm run dev
```

### 3. Test Flow

#### A. Login as Admin
1. Open http://localhost:5173
2. Login with admin credentials
3. Navigate to Admin section

#### B. Navigate to Promos
1. Click sidebar: "Credits & Promos"
2. Click "Promos" tab
3. You should see the promo management page

#### C. Create First Promo
1. Click "Create New Promo" button
2. Fill form:
   ```
   Code: WELCOME20
   Name: Welcome Bonus
   Description: Get 20% bonus credits
   Type: Percentage
   Percentage Bonus: 20
   Min Purchase: 100
   Start Date: Today
   End Date: +30 days
   ```
3. Click "Create Promo"
4. ✅ Success toast appears
5. ✅ Promo appears in table

#### D. View Statistics
1. Find your promo in table
2. Click statistics icon (chart)
3. View usage stats (initially 0)

#### E. Toggle Status
1. Click "Deactivate" button
2. Confirm action
3. ✅ Status changes to "Inactive"
4. Click "Activate" to reactivate

#### F. Edit Promo
1. Click "Edit" button
2. Change name to "Welcome Bonus - 20% Off"
3. Click "Update Promo"
4. ✅ Changes saved

#### G. Test All Promo Types

**Percentage Bonus:**
```
Code: STUDENT15
Type: Percentage
Percentage: 15%
Min Purchase: 50
```

**Fixed Amount:**
```
Code: BONUS50
Type: FixedAmount
Fixed Amount: 50
Min Purchase: 200
```

**Buy X Get Y:**
```
Code: BUY100GET20
Type: BuyXGetY
Buy Amount: 100
Get Amount: 20
```

---

## 📋 Test Checklist

### Navigation
- [ ] Can navigate to Credits page
- [ ] Can click Promos tab
- [ ] Tab navigation works both ways

### Create Operations
- [ ] Can open create modal
- [ ] Can fill all fields
- [ ] Validation shows errors
- [ ] Can create Percentage promo
- [ ] Can create FixedAmount promo
- [ ] Can create BuyXGetY promo
- [ ] Success toast appears

### Read Operations
- [ ] Promos load in table
- [ ] Can search promos
- [ ] Can sort by columns
- [ ] Status badges show correctly
- [ ] Usage counts display

### Update Operations
- [ ] Can open edit modal
- [ ] Can modify fields
- [ ] Can't edit promo code
- [ ] Changes save correctly
- [ ] Toast notification works

### Delete Operations
- [ ] Can click delete
- [ ] Confirmation dialog appears
- [ ] Promo deletes successfully
- [ ] Table updates

### Status Toggle
- [ ] Can activate promo
- [ ] Can deactivate promo
- [ ] Status changes immediately
- [ ] Badge updates

### Statistics
- [ ] Can view statistics
- [ ] Stats modal opens
- [ ] Data displays correctly
- [ ] Shows usage metrics

### Filters
- [ ] Can toggle include inactive
- [ ] Inactive promos show/hide
- [ ] Filter persists

---

## 🎯 API Test Endpoints

### Test with curl or Postman:

```bash
# Get all promos (admin)
curl -X GET http://localhost:5000/api/admin/promos \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create promo
curl -X POST http://localhost:5000/api/admin/promos/create \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "TEST20",
    "name": "Test Promo",
    "type": "Percentage",
    "percentageBonus": 20,
    "minPurchaseAmount": 100
  }'

# Validate promo (user)
curl -X POST http://localhost:5000/api/user/promos/validate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "promoCode": "TEST20",
    "purchaseAmount": 150
  }'
```

---

## 🐛 Common Issues

### Issue: Promos don't load
```bash
# Check backend is running
curl http://localhost:5000/api/admin/promos

# Check JWT token
localStorage.getItem('auth_token')

# Check console for errors
F12 → Console tab
```

### Issue: Create fails
```bash
# Check required fields:
- Code (required, unique)
- Name (required)
- Type (required)
- Type-specific fields

# Check backend logs
dotnet run → watch console
```

### Issue: Navigation doesn't work
```bash
# Check route exists
grep "credits/promos" src/App.tsx

# Check history hook imported
grep "useHistory" src/pages/CreditsManagementPromos.tsx
```

---

## 📊 Success Criteria

### ✅ You Should See:
1. Promo management page loads
2. Table displays with columns
3. Create modal opens
4. Form validation works
5. Promos save to database
6. Statistics display correctly
7. Status toggles work
8. Delete confirmation shows
9. Toast notifications appear
10. Navigation works smoothly

---

## 🎯 Demo Promos to Create

### 1. New User Welcome
```
Code: NEWUSER25
Name: New User Welcome - 25% Bonus
Type: Percentage
Bonus: 25%
Min Purchase: 50
Usage Per User: 1
```

### 2. Weekend Special
```
Code: WEEKEND15
Name: Weekend Special
Type: Percentage
Bonus: 15%
Min Purchase: 100
Start: Next Friday
End: Next Sunday
```

### 3. Loyalty Reward
```
Code: LOYAL500
Name: Loyalty Rewards
Type: BuyXGetY
Buy: 500
Get: 100
Usage Per User: 3
```

### 4. Flash Sale
```
Code: FLASH50
Name: Flash Sale - 50 Credits
Type: FixedAmount
Bonus: 50
Min Purchase: 200
Usage Limit: 100
```

### 5. Student Discount
```
Code: STUDENT10
Name: Student Discount
Type: Percentage
Bonus: 10%
Min Purchase: 30
```

---

## 📸 Expected Screenshots

### Main Page
- ✅ Tab segment (Packages | Promos)
- ✅ Create button
- ✅ Toggle inactive button
- ✅ Promo table with data

### Create Modal
- ✅ All form fields
- ✅ Type-specific fields show/hide
- ✅ Date pickers
- ✅ Create button

### Table View
- ✅ Promos with status badges
- ✅ Action buttons
- ✅ Usage counts
- ✅ Expiry dates

### Statistics Modal
- ✅ Usage metrics
- ✅ Total bonus given
- ✅ Unique users
- ✅ Purchase amounts

---

## 🎊 Final Verification

Run through this flow:
1. ✅ Navigate to promos page
2. ✅ Create 3 different promo types
3. ✅ Edit one promo
4. ✅ Toggle status on another
5. ✅ View statistics
6. ✅ Search for promo
7. ✅ Delete a promo
8. ✅ Switch back to packages tab

If all work → **Implementation is successful!** 🎉

---

## 🚀 You're Ready!

The promo system is fully functional. You can now:
- Create promotional campaigns
- Manage promo codes
- Track usage statistics
- Control promo lifecycle

**Happy promoting!** 🎁

