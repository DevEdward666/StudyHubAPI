# ⚙️ Global Settings - Quick Reference

## 🚀 Quick Start

```bash
# 1. Start backend
cd Study-Hub && dotnet run

# 2. Initialize defaults
POST /api/admin/settings/initialize-defaults

# 3. Ready to use!
```

---

## 📋 Common Operations

### Get All Settings
```bash
GET /api/admin/settings
```

### Get by Key
```bash
GET /api/admin/settings/key/system.maintenance_mode
```

### Get by Category
```bash
GET /api/admin/settings/category/system
```

### Create Setting
```bash
POST /api/admin/settings/create
{
  "key": "your.setting.key",
  "value": "your_value",
  "dataType": "string",
  "category": "your_category"
}
```

### Update Setting
```bash
PUT /api/admin/settings/update
{
  "id": "guid",
  "value": "new_value",
  "changeReason": "optional reason"
}
```

### View History
```bash
GET /api/admin/settings/{id}/history
```

---

## 🎯 Data Types

| Type | Example Value | Validation |
|------|---------------|------------|
| string | "Study Hub" | Regex pattern |
| number | "100" | Min/max range |
| boolean | "true" or "false" | Exact match |
| json | '{"key":"value"}' | Valid JSON |

---

## 📊 Default Settings

### System
- `system.maintenance_mode` → "false"
- `system.app_name` → "Study Hub"
- `system.max_upload_size_mb` → "10"

### Credits
- `credits.default_package_amount` → "100"
- `credits.min_purchase_amount` → "50"

### Notifications
- `notifications.enabled` → "true"
- `notifications.email_enabled` → "true"

### Tables
- `tables.default_hourly_rate` → "50"
- `tables.max_booking_hours` → "8"

---

## 💻 Code Usage

### Get String Value
```csharp
var appName = await _globalSettingsService
    .GetSettingValueAsync("system.app_name");
```

### Get Typed Value
```csharp
var maxUpload = await _globalSettingsService
    .GetSettingValueAsync<int>("system.max_upload_size_mb");

var isEnabled = await _globalSettingsService
    .GetSettingValueAsync<bool>("notifications.enabled");
```

### Set Value
```csharp
await _globalSettingsService.SetSettingValueAsync(
    adminUserId, 
    "system.maintenance_mode", 
    "true",
    "Emergency maintenance"
);
```

---

## 🔧 Categories

- **system** - System configuration
- **credits** - Credit-related settings
- **notifications** - Notification preferences
- **tables** - Table management settings
- **payment** - Payment configuration
- **features** - Feature flags
- **security** - Security settings
- **limits** - Rate limits & restrictions

---

## ✅ Validation Examples

### Number with Range
```json
{
  "dataType": "number",
  "minValue": 1,
  "maxValue": 100
}
```

### String with Regex
```json
{
  "dataType": "string",
  "validationRegex": "^[A-Z0-9]+$"
}
```

### Allowed Values
```json
{
  "dataType": "string",
  "allowedValues": "[\"option1\", \"option2\", \"option3\"]"
}
```

---

## 📈 All Endpoints

```
GET    /api/admin/settings
GET    /api/admin/settings/{id}
GET    /api/admin/settings/key/{key}
GET    /api/admin/settings/category/{category}
POST   /api/admin/settings/create
PUT    /api/admin/settings/update
DELETE /api/admin/settings/delete/{id}
POST   /api/admin/settings/bulk-update
POST   /api/admin/settings/initialize-defaults
GET    /api/admin/settings/{id}/history
GET    /api/admin/settings/changes/recent
POST   /api/admin/settings/validate
GET    /api/admin/settings/export
POST   /api/admin/settings/import
GET    /api/admin/settings/summary
```

---

## 🎯 Status: READY! ✅

