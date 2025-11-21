# Session Ended Modal - Testing Checklist

## 🔍 Pre-Testing Setup

- [ ] Backend server is running
- [ ] Frontend dev server is running
- [ ] Logged in as Admin user
- [ ] On an admin page (e.g., `/app/admin/dashboard`)
- [ ] Browser console is open (F12)

## ✅ Quick Manual Test

Add this temporary test button to any admin page:

```tsx
// Add to UserSessions.tsx or Dashboard temporarily
<IonButton 
  color="warning" 
  onClick={() => {
    // Simulate a session ended notification
    const testNotification = {
      id: 'test-' + Date.now(),
      sessionId: 'session-test',
      tableId: 'table-test',
      tableNumber: '5',
      userName: 'Test Customer',
      message: 'Test session ended',
      duration: 2.5,
      amount: 125.00,
      createdAt: new Date().toISOString()
    };
    
    // Trigger the modal directly (for testing)
    console.log('🧪 Testing session ended modal...');
    // You'll need to expose these via a test function
  }}
>
  🧪 Test Session Modal
</IonButton>
```

## 📋 Testing Steps

### Step 1: Verify SignalR Connection
- [ ] Check console for: "Setting up SignalR for admin..."
- [ ] Check console for: "SignalR setup complete"
- [ ] No SignalR connection errors

### Step 2: Test Session Expiry
- [ ] Create a new subscription with 0.02 hours (1.2 minutes)
- [ ] Start a session on a table
- [ ] Wait for cron job (runs every 1 minute)
- [ ] Session should expire within 2-3 minutes

### Step 3: Verify Modal Appears
When session expires, check:

#### Console Logs (in order):
- [ ] 🔔 Session ended notification received
- [ ] 📝 Setting session ended data...
- [ ] 🔊 Playing session ended sound...
- [ ] 🔊 Playing session ended doorbell sound...
- [ ] ✅ Session ended sound played successfully
- [ ] 🚀 Opening session ended modal...
- [ ] 📊 Session ended modal state changed: true
- [ ] 📋 Session data: {...}

#### Visual Elements:
- [ ] Modal appears on screen
- [ ] Orange warning header visible
- [ ] Pulsing orange circle with clock icon
- [ ] Table number displayed (large, orange text)
- [ ] "Session has ended" text visible
- [ ] Customer name shown correctly
- [ ] Duration displayed (e.g., "2.50 hours")
- [ ] Amount shown (e.g., "₱125.00")
- [ ] Yellow warning box with instructions
- [ ] "Got It - Close Alert" button visible
- [ ] "Go to Tables Management" button visible

#### Sound:
- [ ] Doorbell chime plays (3 notes: Ding-Dong-Ding)
- [ ] Voice announces: "Table [X] session has ended"
- [ ] Sound is loud and clear

### Step 4: Test Modal Interactions

#### Try clicking backdrop:
- [ ] Modal does NOT close (expected behavior)

#### Click "Got It - Close Alert":
- [ ] Console shows: "❌ Closing session ended modal..."
- [ ] Console shows: "📊 Session ended modal state changed: false"
- [ ] Modal closes smoothly

#### Re-trigger and click "Go to Tables Management":
- [ ] Modal closes
- [ ] Navigates to `/app/admin/user-sessions`
- [ ] Table list is visible

### Step 5: Test Pulse Animation
- [ ] Orange circle pulses continuously
- [ ] Shadow expands and contracts
- [ ] Animation is smooth (1.5s loop)

## 🐛 Troubleshooting

### Modal doesn't appear?

**Check console for:**
```
🚀 Opening session ended modal...
📊 Session ended modal state changed: true
```

**If missing:**
- Check if SignalR is connected
- Verify cron job is running on backend
- Check if session actually expired

**If present but modal not visible:**
- Check browser console for CSS errors
- Verify modal isn't hidden behind other elements
- Check z-index in dev tools

### Sound doesn't play?

**Check console for:**
```
🔊 Playing session ended doorbell sound...
✅ Session ended sound played successfully
```

**Common issues:**
- User hasn't interacted with page (click anywhere first)
- Browser audio is muted
- Browser has autoplay restrictions
- Audio context is suspended

**Fix:**
- Click anywhere on the page before testing
- Check browser audio settings
- Look for audio errors in console

### Animation not working?

**Check dev tools:**
1. Inspect the pulsing circle element
2. Look for `animation: pulse 1.5s ease-in-out infinite`
3. Check if `@keyframes pulse` is defined in Styles tab

**If not defined:**
- The inline `<style>` tag should handle it
- Check if React rendered the style tag
- View page source to verify

### Data shows "Loading..."?

**This means `sessionEndedData` is null**

**Check console for:**
```
📝 Setting session ended data...
📋 Session data: {...}
```

**If missing:**
- SignalR notification wasn't received
- Backend didn't send notification
- Check cron job is running

## 📊 Success Criteria

All of these should be TRUE:

✅ Modal appears when session expires
✅ Sound plays (doorbell + voice)
✅ All session data displays correctly
✅ Pulse animation works smoothly
✅ Both buttons work correctly
✅ Modal cannot be dismissed by clicking backdrop
✅ Console logs show expected sequence
✅ No errors in console

## 🎯 Final Verification

After confirming everything works:

1. **Production Ready?**
   - [ ] All tests passed
   - [ ] No console errors
   - [ ] Sound works on different devices
   - [ ] Modal responsive on mobile/tablet

2. **Optional Cleanup:**
   - [ ] Remove test button (if added)
   - [ ] Remove debug console.logs (or keep for monitoring)
   - [ ] Update documentation

3. **Deployment:**
   - [ ] Commit changes
   - [ ] Deploy to staging
   - [ ] Test on staging
   - [ ] Deploy to production

## 📞 Need Help?

If modal still broken after following this checklist:

1. Share console logs (full sequence)
2. Share screenshot of modal (if partially working)
3. Share network tab (SignalR messages)
4. Confirm: Admin user? On admin page? SignalR connected?

---

**Last Updated**: November 21, 2025
**Status**: Ready for Testing ✅

