# Push Notification System Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        StudyHub Ecosystem                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐              ┌──────────────────┐         │
│  │   Client App     │◄────────────►│   Backend API    │         │
│  │  (React/Ionic)   │   REST API   │   (ASP.NET)      │         │
│  └──────────────────┘              └──────────────────┘         │
│           │                                  │                    │
│           │                                  │                    │
│           ▼                                  ▼                    │
│  ┌──────────────────┐              ┌──────────────────┐         │
│  │ Service Worker   │              │   PostgreSQL     │         │
│  │    (sw.js)       │              │   Database       │         │
│  └──────────────────┘              └──────────────────┘         │
│           │                                                       │
│           │                                                       │
│           ▼                                                       │
│  ┌──────────────────┐                                           │
│  │  Browser Push    │                                           │
│  │   Notification   │                                           │
│  └──────────────────┘                                           │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Component Details

### Frontend (Client App)
```
study_hub_app/
├── src/
│   ├── services/
│   │   └── push-notification.service.ts
│   │       ├── registerServiceWorker()
│   │       ├── subscribe()
│   │       ├── unsubscribe()
│   │       └── showLocalNotification()
│   │
│   ├── hooks/
│   │   └── usePushNotification.ts
│   │       ├── permission state
│   │       ├── subscription state
│   │       └── methods
│   │
│   ├── components/notifications/
│   │   ├── PushNotificationSettings.tsx
│   │   └── PushNotificationInitializer.tsx
│   │
│   └── pages/dashboard/
│       ├── TableScanner.tsx  ← Triggers on session start
│       └── Dashboard.tsx     ← Triggers on session end
│
└── public/
    └── sw.js  ← Handles push events
```

### Backend (API)
```
Study-Hub/
├── Controllers/
│   └── PushController.cs
│       ├── GET  /api/push/vapid-public-key
│       ├── POST /api/push/subscribe
│       ├── POST /api/push/unsubscribe
│       ├── GET  /api/push/subscriptions
│       ├── POST /api/push/test
│       ├── POST /api/push/send
│       └── POST /api/push/broadcast
│
├── Service/
│   └── PushNotificationService.cs
│       ├── SubscribeAsync()
│       ├── UnsubscribeAsync()
│       ├── SendPushNotificationAsync()
│       └── SendPushNotificationToUsersAsync()
│
└── Models/
    ├── Entities/PushSubscription.cs
    └── DTOs/PushDto.cs
```

## Data Flow Diagrams

### 1. Initial Setup Flow
```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│  User   │────►│   App   │────►│ Service │────►│ Browser │
│ Opens   │     │  Loads  │     │ Worker  │     │ Prompt  │
│  App    │     │         │     │Register │     │Permission│
└─────────┘     └─────────┘     └─────────┘     └─────────┘
                                                       │
                                                       ▼
                                                 ┌─────────┐
                                                 │  User   │
                                                 │Grants or│
                                                 │ Denies  │
                                                 └─────────┘
                                                       │
                                 ┌─────────────────────┤
                                 │                     │
                           [Granted]              [Denied]
                                 │                     │
                                 ▼                     ▼
                        ┌──────────────┐      ┌──────────────┐
                        │ Subscribe to │      │Show Settings │
                        │   Backend    │      │ Instructions │
                        └──────────────┘      └──────────────┘
```

### 2. Session Start Flow
```
User Scans QR Code
       │
       ▼
┌──────────────┐
│TableScanner  │
│  Component   │
└──────────────┘
       │
       ▼
[Confirm Session Details]
       │
       ▼
┌──────────────┐
│Start Session │
│  API Call    │
└──────────────┘
       │
       ▼
[Success]
       │
       ├────────────────────────┐
       │                        │
       ▼                        ▼
┌──────────────┐      ┌──────────────────┐
│ Navigate to  │      │ Send Local Push  │
│  Dashboard   │      │   Notification   │
└──────────────┘      └──────────────────┘
                               │
                               ▼
                      ┌──────────────────┐
                      │ Service Worker   │
                      │ Shows Notification│
                      └──────────────────┘
                               │
                               ▼
                      ┌──────────────────┐
                      │ User Sees:       │
                      │ "Study Session   │
                      │  Started! 🎯"    │
                      └──────────────────┘
```

### 3. Session End Flow
```
User Clicks "End Session"
       │
       ▼
┌──────────────┐
│  Dashboard   │
│  Component   │
└──────────────┘
       │
       ▼
[Calculate Duration]
       │
       ▼
┌──────────────┐
│ End Session  │
│   API Call   │
└──────────────┘
       │
       ▼
[Success]
       │
       ├────────────────────────┐
       │                        │
       ▼                        ▼
┌──────────────┐      ┌──────────────────┐
│Refresh Data  │      │ Send Local Push  │
│  & UI        │      │   Notification   │
└──────────────┘      └──────────────────┘
                               │
                               ▼
                      ┌──────────────────┐
                      │ Service Worker   │
                      │ Shows Notification│
                      └──────────────────┘
                               │
                               ▼
                      ┌──────────────────┐
                      │ User Sees:       │
                      │ "Session         │
                      │  Completed! 🎉"  │
                      └──────────────────┘
```

### 4. Subscription Flow
```
┌─────────┐     GET /vapid-public-key      ┌─────────┐
│ Client  │──────────────────────────────►│ Backend │
└─────────┘                                └─────────┘
     │                                           │
     │◄──────────────────────────────────────────┤
     │        { publicKey: "..." }               │
     │                                           │
     ▼                                           │
┌─────────┐                                     │
│Subscribe│                                     │
│  to     │                                     │
│Browser  │                                     │
│Push API │                                     │
└─────────┘                                     │
     │                                           │
     │        POST /subscribe                   │
     │       { endpoint, keys }                 │
     ├──────────────────────────────────────────►
     │                                           │
     │                                           ▼
     │                                    ┌─────────┐
     │                                    │  Save   │
     │                                    │   to    │
     │                                    │Database │
     │                                    └─────────┘
     │                                           │
     │◄──────────────────────────────────────────┤
     │           Success Response                │
     ▼                                           │
┌─────────┐                                     │
│  User   │                                     │
│is now   │                                     │
│subscribed│                                    │
└─────────┘                                     │
```

## Database Schema

```sql
┌──────────────────────────────────────────────┐
│          push_subscriptions                  │
├──────────────────────────────────────────────┤
│ id              UUID (PK)                    │
│ user_id         UUID (FK -> users)           │
│ endpoint        VARCHAR(500) UNIQUE          │
│ p256dh          VARCHAR(200)                 │
│ auth            VARCHAR(200)                 │
│ is_active       BOOLEAN                      │
│ user_agent      VARCHAR(500)                 │
│ created_at      TIMESTAMP                    │
│ updated_at      TIMESTAMP                    │
│ last_used_at    TIMESTAMP                    │
└──────────────────────────────────────────────┘
         │
         │ Indexes:
         ├─ IX_push_subscriptions_user_id
         ├─ IX_push_subscriptions_endpoint (UNIQUE)
         └─ IX_push_subscriptions_is_active
```

## Security Flow

```
┌─────────────────────────────────────────────┐
│           VAPID Authentication              │
├─────────────────────────────────────────────┤
│                                             │
│  1. Backend generates VAPID key pair        │
│     (Public + Private key)                  │
│                                             │
│  2. Public key shared with client           │
│     (Safe to expose)                        │
│                                             │
│  3. Private key stays on backend            │
│     (Never exposed)                         │
│                                             │
│  4. Client uses public key to subscribe     │
│     to browser push service                 │
│                                             │
│  5. Backend uses private key to sign        │
│     push messages (proves authenticity)     │
│                                             │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│         JWT Authentication Flow             │
├─────────────────────────────────────────────┤
│                                             │
│  All API calls include:                     │
│  Authorization: Bearer <JWT_TOKEN>          │
│                                             │
│  Protected endpoints:                       │
│  - POST /subscribe                          │
│  - POST /unsubscribe                        │
│  - GET  /subscriptions                      │
│  - POST /test                               │
│  - POST /send (Admin only)                  │
│  - POST /broadcast (Admin only)             │
│                                             │
└─────────────────────────────────────────────┘
```

## Technology Stack

```
┌─────────────────────────────────────────────┐
│              Frontend Stack                 │
├─────────────────────────────────────────────┤
│ • React 19.0.0                              │
│ • Ionic Framework 8.7.3                     │
│ • TypeScript 5.7.2                          │
│ • Vite 6.2.0                                │
│ • Service Workers API                       │
│ • Push API                                  │
│ • Notifications API                         │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│              Backend Stack                  │
├─────────────────────────────────────────────┤
│ • ASP.NET Core 8.0                          │
│ • C# / .NET 8                               │
│ • Entity Framework Core 9.0.9               │
│ • PostgreSQL                                │
│ • Lib.Net.Http.WebPush 3.3.1                │
│ • JWT Authentication                        │
└─────────────────────────────────────────────┘
```

## Notification Payload Structure

```json
{
  "notification": {
    "title": "Study Session Started! 🎯",
    "body": "Your 2 hour study session at Building A - Table 5 has begun.",
    "icon": "/icon.png",
    "badge": "/badge.png",
    "tag": "session-start",
    "data": {
      "type": "session-start",
      "tableId": "uuid-here",
      "tableNumber": "5",
      "location": "Building A",
      "duration": 2
    }
  }
}
```

## Performance Metrics

```
┌─────────────────────────────────────────────┐
│         Expected Performance                │
├─────────────────────────────────────────────┤
│                                             │
│ Service Worker Registration: < 100ms        │
│ Permission Request: Instant                 │
│ Subscription to Backend: < 500ms            │
│ Local Notification Display: < 50ms          │
│ API Response Time: < 200ms                  │
│                                             │
│ Browser Support: 95%+ of users              │
│ Mobile Support: iOS 16.4+, Android 5.0+     │
│                                             │
└─────────────────────────────────────────────┘
```

## Error Handling Flow

```
┌─────────────┐
│ Try Action  │
└─────────────┘
       │
       ▼
   [Success?]
       │
       ├──────Yes──────►[Continue]
       │
       ▼
      No
       │
       ▼
┌─────────────┐
│ Catch Error │
└─────────────┘
       │
       ├──────►[Log to Console]
       │
       ├──────►[Show User Message (optional)]
       │
       └──────►[Graceful Degradation]
                (App continues to work)
```

## State Management

```
┌──────────────────────────────────────┐
│      Push Notification States        │
├──────────────────────────────────────┤
│                                      │
│ • isSupported: boolean               │
│ • permission: NotificationPermission │
│   - "default"                        │
│   - "granted"                        │
│   - "denied"                         │
│                                      │
│ • isSubscribed: boolean              │
│ • isLoading: boolean                 │
│ • error: string | null               │
│                                      │
└──────────────────────────────────────┘
```

---

## Summary

This architecture provides:
- ✅ End-to-end push notification system
- ✅ Secure VAPID authentication
- ✅ JWT-protected API endpoints
- ✅ Persistent subscription storage
- ✅ Service worker for offline support
- ✅ Graceful error handling
- ✅ Cross-browser compatibility
- ✅ Mobile-friendly implementation

**Status: Production Ready** 🚀

