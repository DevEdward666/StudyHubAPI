# Web Push Notifications Setup Guide

## Overview
This guide explains how to use the web push notification feature in StudyHub API using the Lib.Net.Http.WebPush library.

## Features Implemented

1. **Push Subscription Management**
   - Subscribe users to push notifications
   - Unsubscribe users from push notifications
   - List user subscriptions
   - Automatic cleanup of inactive subscriptions

2. **Push Notification Sending**
   - Send push to individual users
   - Send push to multiple users
   - Broadcast push to all users
   - Test push notifications

3. **VAPID Authentication**
   - Secure VAPID key management
   - Public key endpoint for client subscription

## API Endpoints

### 1. Get VAPID Public Key
```
GET /api/push/vapid-public-key
```
**Auth**: None  
**Response**: 
```json
{
  "publicKey": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE..."
}
```

### 2. Subscribe to Push Notifications
```
POST /api/push/subscribe
```
**Auth**: Required  
**Body**:
```json
{
  "endpoint": "https://fcm.googleapis.com/fcm/send/...",
  "keys": {
    "p256dh": "BNJ...",
    "auth": "tB..."
  },
  "userAgent": "Mozilla/5.0 ..."
}
```

### 3. Unsubscribe from Push Notifications
```
POST /api/push/unsubscribe
```
**Auth**: Required  
**Body**: `"https://fcm.googleapis.com/fcm/send/..."`

### 4. Get User Subscriptions
```
GET /api/push/subscriptions
```
**Auth**: Required

### 5. Send Test Push
```
POST /api/push/test
```
**Auth**: Required  
**Body**:
```json
{
  "userId": "guid",
  "title": "Test Notification",
  "body": "This is a test"
}
```

### 6. Send Push to Specific Users (Admin Only)
```
POST /api/push/send
```
**Auth**: Admin/SuperAdmin  
**Body**:
```json
{
  "userIds": ["guid1", "guid2"],
  "notification": {
    "title": "Important Update",
    "body": "Check out the new features!",
    "icon": "/icon.png",
    "url": "/dashboard"
  }
}
```

### 7. Broadcast Push to All Users (Admin Only)
```
POST /api/push/broadcast
```
**Auth**: Admin/SuperAdmin  
**Body**:
```json
{
  "title": "System Announcement",
  "body": "Maintenance scheduled for tonight",
  "icon": "/icon.png",
  "badge": "/badge.png",
  "priority": "High"
}
```

## Client-Side Implementation

### 1. Request Notification Permission
```javascript
async function requestNotificationPermission() {
  const permission = await Notification.requestPermission();
  if (permission === 'granted') {
    await subscribeUser();
  }
}
```

### 2. Subscribe User
```javascript
async function subscribeUser() {
  try {
    // Get VAPID public key
    const response = await fetch('/api/push/vapid-public-key');
    const { publicKey } = await response.json();
    
    // Register service worker
    const registration = await navigator.serviceWorker.register('/sw.js');
    
    // Subscribe to push
    const subscription = await registration.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(publicKey)
    });
    
    // Send subscription to server
    await fetch('/api/push/subscribe', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        endpoint: subscription.endpoint,
        keys: {
          p256dh: arrayBufferToBase64(subscription.getKey('p256dh')),
          auth: arrayBufferToBase64(subscription.getKey('auth'))
        },
        userAgent: navigator.userAgent
      })
    });
  } catch (error) {
    console.error('Failed to subscribe:', error);
  }
}

function urlBase64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - base64String.length % 4) % 4);
  const base64 = (base64String + padding)
    .replace(/\-/g, '+')
    .replace(/_/g, '/');
  
  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);
  
  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

function arrayBufferToBase64(buffer) {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return window.btoa(binary)
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}
```

### 3. Service Worker (sw.js)
```javascript
self.addEventListener('push', event => {
  const data = event.data.json();
  const options = {
    body: data.notification.body,
    icon: data.notification.icon || '/icon.png',
    badge: data.notification.badge || '/badge.png',
    image: data.notification.image,
    data: data.notification.data,
    actions: data.notification.actions || [],
    tag: data.notification.tag,
    requireInteraction: false
  };
  
  event.waitUntil(
    self.registration.showNotification(data.notification.title, options)
  );
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  
  const urlToOpen = event.notification.data?.url || '/';
  
  event.waitUntil(
    clients.openWindow(urlToOpen)
  );
});
```

## Database Schema

### PushSubscriptions Table
```sql
CREATE TABLE push_subscriptions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    endpoint VARCHAR(500) NOT NULL UNIQUE,
    p256dh VARCHAR(200),
    auth VARCHAR(200),
    is_active BOOLEAN DEFAULT TRUE,
    user_agent VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_used_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_push_subscriptions_user_id ON push_subscriptions(user_id);
CREATE INDEX idx_push_subscriptions_is_active ON push_subscriptions(is_active);
```

## Configuration

Add these settings to `appsettings.json`:

```json
{
  "WebPush": {
    "VapidPublicKey": "YOUR_GENERATED_PUBLIC_KEY",
    "VapidPrivateKey": "YOUR_GENERATED_PRIVATE_KEY",
    "VapidSubject": "mailto:admin@studyhub.com"
  }
}
```

## Generating VAPID Keys

Use the VapidKeyGen console app:

```bash
cd VapidKeyGen
dotnet run
```

This will generate new VAPID keys that you can add to your appsettings.json.

## Integration with Notification System

You can automatically send push notifications when creating database notifications:

```csharp
// In NotificationService
public async Task<Notification> CreateNotificationAsync(Guid userId, NotificationDto dto)
{
    // Create database notification
    var notification = await _context.Notifications.AddAsync(...);
    await _context.SaveChangesAsync();
    
    // Send push notification
    var pushNotification = new PushNotificationDto
    {
        Title = dto.Title,
        Body = dto.Message,
        Icon = "/icon.png",
        Url = dto.ActionUrl,
        Data = new Dictionary<string, object>
        {
            ["notificationId"] = notification.Id,
            ["type"] = dto.Type.ToString()
        }
    };
    
    await _pushService.SendPushNotificationAsync(userId, pushNotification);
    
    return notification;
}
```

## Testing

### Test with curl:
```bash
# Get VAPID public key
curl http://localhost:5212/api/push/vapid-public-key

# Send test push (replace TOKEN and USER_ID)
curl -X POST http://localhost:5212/api/push/test \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "USER_GUID",
    "title": "Test Push",
    "body": "This is a test notification"
  }'
```

## Best Practices

1. **Always check notification permission** before subscribing
2. **Handle subscription updates** when they change
3. **Implement retry logic** for failed push deliveries
4. **Clean up inactive subscriptions** periodically
5. **Use meaningful notification tags** to prevent duplicates
6. **Respect user preferences** - allow users to mute notifications
7. **Test on multiple browsers** (Chrome, Firefox, Edge, Safari)

## Browser Compatibility

- ✅ Chrome 50+
- ✅ Firefox 44+
- ✅ Edge 17+
- ✅ Safari 16+ (macOS 13+, iOS 16.4+)
- ✅ Opera 37+

## Troubleshooting

### Notifications not received
1. Check if service worker is registered
2. Verify VAPID keys are correct
3. Check browser console for errors
4. Verify subscription is in database
5. Check if notification permission is granted

### Subscription fails
1. Ensure HTTPS is being used (or localhost for testing)
2. Check if service worker path is correct
3. Verify VAPID public key format

### Push delivery fails
1. Check if subscription is still valid (410 Gone)
2. Verify VAPID subject matches domain
3. Check payload size (max 4KB)

## Security Considerations

1. **Never expose private VAPID key** to clients
2. **Use HTTPS** in production
3. **Validate user authorization** before sending push
4. **Implement rate limiting** to prevent abuse
5. **Sanitize notification content** to prevent XSS

## Performance Tips

1. **Batch notifications** when sending to multiple users
2. **Use background jobs** for large broadcasts
3. **Implement cleanup job** for old subscriptions
4. **Cache VAPID keys** instead of reading config repeatedly
5. **Use connection pooling** for HTTP client

## License

This implementation uses Lib.Net.Http.WebPush (MIT License)

