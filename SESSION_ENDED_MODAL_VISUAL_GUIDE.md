# Session Ended Modal - Visual Guide

## 🎨 Modal Appearance

```
┌─────────────────────────────────────────────────────┐
│  ⚠️  Session Ended Alert                      [×]   │ ← Warning Orange Header
├─────────────────────────────────────────────────────┤
│                                                     │
│                    ⏰                                │ ← Pulsing Circle
│                  (pulsing)                          │   (Animated)
│               Orange Gradient                       │
│                                                     │
│  ┌───────────────────────────────────────────────┐ │
│  │          Table 5                              │ │ ← Large Display
│  │       Session has ended                       │ │
│  │                                               │ │
│  │   Customer:     John Doe                      │ │
│  │   Duration:     2.50 hours                    │ │
│  │   Amount:       ₱125.00                       │ │
│  │                                               │ │
│  │   ⚠️ Please check on the customer and         │ │ ← Action Reminder
│  │      prepare the table for the next session   │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  ┌───────────────────────────────────────────────┐ │
│  │         Got It - Close Alert                  │ │ ← Primary Button
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  ┌───────────────────────────────────────────────┐ │
│  │      Go to Tables Management                  │ │ ← Secondary Button
│  └───────────────────────────────────────────────┘ │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## 🔊 Sound Sequence

```
Timeline:
0.0s  ──────┐ 🔔 Ding (C5)
0.2s  ──────┼─────┐ 🔔 Dong (E5)
0.4s  ──────┼─────┼────────┐ 🔔 Ding (G5, longer)
       ─────┼─────┼────────┼──────┐
            └─────┴────────┴──────┘ (Reverb echo)
0.8s                              🗣️ "Table 5 session has ended"

Total duration: ~2 seconds
```

## 📱 Responsive Sizes

### Mobile (< 768px)
- Width: 90% of screen
- Max width: 500px
- Compact layout

### Tablet/Desktop (≥ 768px)
- Fixed width: 600px
- Larger spacing
- More comfortable layout

## 🎨 Color Scheme

- **Header**: `--ion-color-warning` (Orange/Yellow)
- **Icon Background**: Linear gradient `#FF6B6B → #FFB74D`
- **Table Number**: `--ion-color-warning`
- **Duration**: `--ion-color-primary`
- **Amount**: `--ion-color-success`
- **Warning Box**: `#FFF3CD` background, `#FFC107` border
- **Primary Button**: Gradient primary → secondary

## ⚙️ Animation Details

### Pulse Animation
```css
0% & 100%  → Scale: 1.0,  Shadow: 0px
50%        → Scale: 1.05, Shadow: 20px expanding circle

Loop: Infinite, 1.5s duration
Easing: ease-in-out
```

## 🔐 Modal Behavior

- ❌ Cannot click backdrop to dismiss
- ✅ Must click button to close
- ✅ Escape key still works (browser default)
- ✅ Stacks on top of all other UI elements
- ✅ Scrollable if content exceeds viewport

## 📊 Comparison: Toast vs Modal

| Feature | Toast (Old) | Modal (New) |
|---------|-------------|-------------|
| Visibility | Top of screen, 8s | Full screen, stays until dismissed |
| Dismissal | Auto or click X | Must click button |
| Information | Single line | Full details with formatting |
| Sound | ✅ | ✅ Enhanced (louder + speech) |
| Actions | None | 2 buttons (dismiss or navigate) |
| Attention | Medium | **High** |
| Stackable | Yes | One at a time |

## 🎯 Use Case Example

**Scenario**: Busy coffee shop, 3pm rush hour

1. **Customer at Table 5** has 10 minutes left
2. **Timer reaches zero**
3. **Cron job** (runs every 1 minute) detects expired session
4. **Backend** ends session, sends SignalR notification
5. **Admin's device**:
   - 🔊 **Sound plays** (3-note doorbell + voice)
   - 📱 **Modal pops up** (can't miss it!)
   - 👀 **Admin sees**: Table 5, John Doe, 2.5hrs, ₱125
6. **Admin acknowledges**:
   - Clicks "Got It"
   - Goes to Table 5
   - Processes payment
   - Clears table
7. **Table ready** for next customer

## 🚀 Benefits

1. **Impossible to Miss**: Full-screen modal with sound
2. **All Info at a Glance**: No need to check other pages
3. **Quick Action**: Navigate directly to tables page
4. **Professional**: Polished UI matches rest of admin panel
5. **Accessible**: Both visual and audio alerts

