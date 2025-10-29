# Global Settings - Quick Reference

## Common Settings to Configure

### Payment Settings
```typescript
// Fixed rate for table usage
{
  key: "payment.fixed_rate",
  value: "25",
  dataType: "number",
  category: "payment",
  description: "Fixed rate amount in PHP for table usage"
}

// GCash payment enabled
{
  key: "payment.gcash_enabled",
  value: "true",
  dataType: "boolean",
  category: "payment",
  description: "Enable GCash payment method"
}

// Minimum purchase amount
{
  key: "payment.minimum_amount",
  value: "10",
  dataType: "number",
  category: "payment",
  minValue: 1,
  description: "Minimum credit purchase amount in PHP"
}
```

### Feature Flags
```typescript
// WiFi portal feature
{
  key: "feature.wifi_portal_enabled",
  value: "true",
  dataType: "boolean",
  category: "feature",
  description: "Enable WiFi portal access system"
}

// Promo codes feature
{
  key: "feature.promo_codes_enabled",
  value: "true",
  dataType: "boolean",
  category: "feature",
  description: "Enable promotional codes system"
}

// Notifications feature
{
  key: "feature.push_notifications_enabled",
  value: "true",
  dataType: "boolean",
  category: "feature",
  description: "Enable push notifications"
}
```

### Notification Settings
```typescript
// Email notifications
{
  key: "notification.email_enabled",
  value: "true",
  dataType: "boolean",
  category: "notification",
  description: "Enable email notifications"
}

// SMS notifications
{
  key: "notification.sms_enabled",
  value: "false",
  dataType: "boolean",
  category: "notification",
  description: "Enable SMS notifications"
}

// Push notification settings
{
  key: "notification.push_batch_size",
  value: "100",
  dataType: "number",
  category: "notification",
  description: "Number of notifications to send per batch"
}
```

### System Settings
```typescript
// Session timeout
{
  key: "system.session_timeout_minutes",
  value: "120",
  dataType: "number",
  category: "system",
  minValue: 15,
  maxValue: 480,
  description: "User session timeout in minutes"
}

// Maintenance mode
{
  key: "system.maintenance_mode",
  value: "false",
  dataType: "boolean",
  category: "system",
  description: "Enable maintenance mode (disable user access)"
}

// API rate limit
{
  key: "system.api_rate_limit",
  value: "100",
  dataType: "number",
  category: "system",
  description: "API requests per minute per user"
}
```

## Code Examples

### Frontend - Reading a Setting Value

```typescript
import globalSettingsService from '@/services/global-settings.service';

// Get a specific setting value
async function getFixedRate() {
  try {
    const value = await globalSettingsService.getSettingValue('payment.fixed_rate');
    console.log('Fixed rate:', value);
    return parseFloat(value);
  } catch (error) {
    console.error('Failed to get fixed rate:', error);
    return 25; // fallback value
  }
}

// Get all settings in a category
async function getPaymentSettings() {
  try {
    const settings = await globalSettingsService.getSettingsByCategory('payment');
    return settings;
  } catch (error) {
    console.error('Failed to get payment settings:', error);
    return [];
  }
}

// Update a setting
async function updateFixedRate(newRate: number) {
  try {
    await globalSettingsService.updateSettingByKey(
      'payment.fixed_rate',
      newRate.toString(),
      'Updated fixed rate for new pricing'
    );
    console.log('Fixed rate updated successfully');
  } catch (error) {
    console.error('Failed to update fixed rate:', error);
  }
}
```

### Backend - Reading a Setting in C#

```csharp
// In a controller or service
private readonly IGlobalSettingsService _settingsService;

public async Task<decimal> GetFixedRate()
{
    var setting = await _settingsService.GetSettingByKeyAsync("payment.fixed_rate");
    if (setting != null && decimal.TryParse(setting.Value, out var rate))
    {
        return rate;
    }
    return 25; // fallback value
}

// Get multiple settings efficiently
public async Task<Dictionary<string, string>> GetPaymentSettings()
{
    var settings = await _settingsService.GetSettingsByCategoryAsync("payment");
    return settings.ToDictionary(s => s.Key, s => s.Value);
}

// Update a setting
public async Task UpdateFixedRate(decimal newRate, Guid adminUserId)
{
    var setting = await _settingsService.GetSettingByKeyAsync("payment.fixed_rate");
    if (setting != null)
    {
        await _settingsService.UpdateSettingAsync(adminUserId, new UpdateGlobalSettingRequestDto
        {
            Id = setting.Id,
            Value = newRate.ToString(),
            ChangeReason = "Updated fixed rate for new pricing"
        });
    }
}
```

## API Quick Reference

### Get Settings
```bash
# Get all settings
GET /admin/settings

# Get by ID
GET /admin/settings/{id}

# Get by key
GET /admin/settings/key/{key}

# Get by category
GET /admin/settings/category/{category}
```

### Create/Update Settings
```bash
# Create setting
POST /admin/settings/create
{
  "key": "feature.new_feature",
  "value": "true",
  "description": "Enable new feature",
  "dataType": "boolean",
  "category": "feature"
}

# Update setting
PUT /admin/settings/update
{
  "id": "guid-here",
  "value": "new_value",
  "changeReason": "Updated because..."
}

# Validate setting
POST /admin/settings/validate
{
  "key": "payment.fixed_rate",
  "value": "50"
}
```

### History
```bash
# Get setting history
GET /admin/settings/{id}/history

# Get recent changes
GET /admin/settings/changes/recent?count=50
```

## UI Navigation

1. **Admin Panel** → **Settings** icon
2. Select **Settings** or **History** tab
3. Use search bar to find specific settings
4. Use category filter to narrow results
5. Click edit icon to modify
6. Click clock icon to view history

## Best Practices

### Naming Convention
Use dot notation for hierarchical organization:
- `category.subcategory.setting_name`
- Examples:
  - `payment.gcash.enabled`
  - `notification.email.smtp_host`
  - `feature.wifi.auto_disconnect_minutes`

### Data Types
Choose appropriate data type:
- **string**: Text, API keys, URLs
- **number**: Numeric values, counts, amounts
- **boolean**: Feature flags, on/off switches
- **json**: Complex configurations, arrays

### Security
- Mark sensitive values as `isEncrypted: true`
- Set `isPublic: false` for admin-only settings
- Use strong validation rules
- Always provide change reasons for audit

### Validation
- Set `minValue` and `maxValue` for numeric settings
- Use `validationRegex` for pattern matching
- Provide `allowedValues` for enumerations
- Add clear descriptions for guidance

## Troubleshooting

### Setting not saving
- Check validation rules (min/max, regex)
- Verify data type matches value
- Ensure you have admin permissions
- Check for duplicate keys

### Value not updating in app
- Clear cache/reload app
- Check if setting is cached
- Verify correct key name
- Check for fallback values in code

### History not showing
- Ensure setting has been modified at least once
- Check date range filters
- Verify admin permissions

## Migration from Hardcoded Values

When moving hardcoded values to global settings:

1. **Identify the value** in code
2. **Create setting** via UI or API
3. **Update code** to read from settings
4. **Add fallback** for safety
5. **Test thoroughly**
6. **Deploy** with proper rollback plan

Example migration:
```typescript
// Before (hardcoded)
const FIXED_RATE = 25;

// After (from settings)
const [fixedRate, setFixedRate] = useState(25); // fallback

useEffect(() => {
  globalSettingsService.getSettingValue('payment.fixed_rate')
    .then(value => setFixedRate(parseFloat(value)))
    .catch(err => console.error('Failed to load rate:', err));
}, []);
```

## Support

For issues or questions about global settings:
1. Check this reference guide
2. Review the full documentation (GLOBAL_SETTINGS_UI_COMPLETE.md)
3. Check backend logs for errors
4. Verify admin authentication
5. Test with the provided test script

---

**Last Updated**: October 29, 2025

