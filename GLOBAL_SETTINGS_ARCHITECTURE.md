# Global Settings Architecture & Flow

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND (React/Ionic)                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  GlobalSettings.tsx (UI Component)                        │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────────────┐ │  │
│  │  │  Settings  │  │  History   │  │  Modals            │ │  │
│  │  │  Tab       │  │  Tab       │  │  - Edit            │ │  │
│  │  │            │  │            │  │  - Create          │ │  │
│  │  │  - List    │  │  - Recent  │  │  - History Detail  │ │  │
│  │  │  - Search  │  │    Changes │  │                    │ │  │
│  │  │  - Filter  │  │  - Details │  │                    │ │  │
│  │  └────────────┘  └────────────┘  └────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↓                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  global-settings.service.ts (API Service)                 │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  - getAllSettings()                                 │  │  │
│  │  │  - getSettingByKey()                                │  │  │
│  │  │  - createSetting()                                  │  │  │
│  │  │  - updateSetting()                                  │  │  │
│  │  │  - getSettingHistory()                              │  │  │
│  │  │  - validateSetting()                                │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↓                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  api.client.ts (HTTP Client)                             │  │
│  │  - Authentication                                         │  │
│  │  - Request/Response handling                             │  │
│  │  - Zod validation                                         │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓ HTTP/HTTPS
                              ↓ Authorization: Bearer <token>
┌─────────────────────────────────────────────────────────────────┐
│                      BACKEND (ASP.NET Core)                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  AdminController.cs                                       │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  GET    /admin/settings                            │  │  │
│  │  │  GET    /admin/settings/{id}                       │  │  │
│  │  │  GET    /admin/settings/key/{key}                  │  │  │
│  │  │  GET    /admin/settings/category/{category}        │  │  │
│  │  │  POST   /admin/settings/create                     │  │  │
│  │  │  PUT    /admin/settings/update                     │  │  │
│  │  │  POST   /admin/settings/validate                   │  │  │
│  │  │  GET    /admin/settings/{id}/history               │  │  │
│  │  │  GET    /admin/settings/changes/recent             │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↓                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  GlobalSettingsService.cs                                 │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  - Business Logic                                   │  │  │
│  │  │  - Validation                                       │  │  │
│  │  │  - Change Tracking                                  │  │  │
│  │  │  - History Management                               │  │  │
│  │  │  - Encryption (if enabled)                          │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↓                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  ApplicationDbContext (EF Core)                           │  │
│  │  - DbSet<GlobalSetting>                                   │  │
│  │  - DbSet<GlobalSettingHistory>                            │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      DATABASE (PostgreSQL)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  global_settings                                          │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  id                  UUID PK                        │  │  │
│  │  │  key                 VARCHAR(100) UNIQUE            │  │  │
│  │  │  value               TEXT                           │  │  │
│  │  │  description         VARCHAR(500)                   │  │  │
│  │  │  data_type           VARCHAR(50)                    │  │  │
│  │  │  category            VARCHAR(100)                   │  │  │
│  │  │  is_public           BOOLEAN                        │  │  │
│  │  │  is_encrypted        BOOLEAN                        │  │  │
│  │  │  validation_regex    VARCHAR(500)                   │  │  │
│  │  │  default_value       TEXT                           │  │  │
│  │  │  min_value           NUMERIC                        │  │  │
│  │  │  max_value           NUMERIC                        │  │  │
│  │  │  allowed_values      TEXT                           │  │  │
│  │  │  updated_by          UUID FK -> users               │  │  │
│  │  │  created_at          TIMESTAMPTZ                    │  │  │
│  │  │  updated_at          TIMESTAMPTZ                    │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  global_settings_history                                  │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  id                  UUID PK                        │  │  │
│  │  │  setting_id          UUID FK -> global_settings     │  │  │
│  │  │  key                 VARCHAR(100)                   │  │  │
│  │  │  old_value           TEXT                           │  │  │
│  │  │  new_value           TEXT                           │  │  │
│  │  │  changed_by          UUID FK -> users               │  │  │
│  │  │  changed_at          TIMESTAMPTZ                    │  │  │
│  │  │  change_reason       VARCHAR(500)                   │  │  │
│  │  │  created_at          TIMESTAMPTZ                    │  │  │
│  │  │  updated_at          TIMESTAMPTZ                    │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow Diagrams

### Creating a New Setting

```
User
  │
  ├─> Clicks "New Setting" button
  │
  ├─> Fills form in Create Modal
  │    - key: "feature.new_feature"
  │    - value: "true"
  │    - dataType: "boolean"
  │    - category: "feature"
  │
  ├─> Clicks "Create Setting"
  │
  ↓
GlobalSettings.tsx
  │
  ├─> Validates form
  │
  ├─> Calls handleCreateSetting()
  │
  ↓
global-settings.service.ts
  │
  ├─> createSetting(request)
  │
  ├─> POST /admin/settings/create
  │    Authorization: Bearer <token>
  │    Body: CreateGlobalSettingRequest
  │
  ↓
AdminController.cs
  │
  ├─> [HttpPost("settings/create")]
  │
  ├─> Validates admin user
  │
  ├─> Calls _globalSettingsService.CreateSettingAsync()
  │
  ↓
GlobalSettingsService.cs
  │
  ├─> Validates setting doesn't exist
  │
  ├─> Validates value against rules
  │
  ├─> Creates GlobalSetting entity
  │
  ├─> Saves to database
  │
  ├─> Returns GlobalSettingDto
  │
  ↓
Database
  │
  ├─> INSERT into global_settings
  │
  └─> Commit transaction
  
Response flows back up the chain
  │
  ├─> GlobalSettingDto
  │
  ├─> API Response validated by Zod
  │
  ├─> UI shows success toast
  │
  ├─> Modal closes
  │
  └─> Settings list refreshes
```

### Updating a Setting

```
User
  │
  ├─> Clicks edit icon on setting
  │
  ├─> Edit Modal opens with current value
  │
  ├─> Changes value: "25" → "30"
  │
  ├─> Adds change reason: "Updated pricing"
  │
  ├─> Clicks "Save Changes"
  │
  ├─> Confirmation dialog appears
  │
  └─> Confirms update
  │
  ↓
GlobalSettings.tsx
  │
  ├─> handleSaveEdit()
  │
  ↓
global-settings.service.ts
  │
  ├─> updateSetting(request)
  │
  ├─> PUT /admin/settings/update
  │    Body: { id, value, changeReason }
  │
  ↓
GlobalSettingsService.cs
  │
  ├─> Gets existing setting
  │
  ├─> Validates new value
  │
  ├─> Creates GlobalSettingHistory entry
  │    - oldValue: "25"
  │    - newValue: "30"
  │    - changedBy: admin user ID
  │    - changeReason: "Updated pricing"
  │
  ├─> Updates GlobalSetting
  │    - value: "30"
  │    - updatedBy: admin user ID
  │    - updatedAt: NOW()
  │
  └─> Saves both to database
  │
  ↓
Database
  │
  ├─> INSERT into global_settings_history
  │
  ├─> UPDATE global_settings SET...
  │
  └─> Commit transaction
  
Response
  │
  ├─> Success message
  │
  ├─> Modal closes
  │
  ├─> Settings refreshed
  │
  └─> History updated
```

### Viewing History

```
User
  │
  ├─> Clicks clock icon on setting
  │
  ↓
GlobalSettings.tsx
  │
  ├─> loadSettingHistory(settingId)
  │
  ↓
global-settings.service.ts
  │
  ├─> getSettingHistory(settingId)
  │
  ├─> GET /admin/settings/{id}/history
  │
  ↓
GlobalSettingsService.cs
  │
  ├─> Queries global_settings_history
  │    WHERE setting_id = {id}
  │    ORDER BY changed_at DESC
  │
  ├─> Joins with users table
  │    (to get changedByEmail)
  │
  └─> Returns List<GlobalSettingHistoryDto>
  │
  ↓
UI
  │
  ├─> History Modal opens
  │
  └─> Displays chronological list:
      - Timestamp
      - Old value → New value
      - Changed by user
      - Reason (if provided)
```

## Component Hierarchy

```
GlobalSettings.tsx
│
├─── Page Header
│    ├─ Title
│    └─ Description
│
├─── Tab Selector (IonSegment)
│    ├─ Settings Tab
│    └─ History Tab
│
├─── Settings Tab Content
│    │
│    ├─── Search & Filter Bar (IonCard)
│    │    ├─ Search Bar
│    │    ├─ Category Dropdown
│    │    └─ New Setting Button
│    │
│    └─── Settings List (IonList)
│         └─ Setting Cards (IonCard) [foreach setting]
│              ├─ Key & Badges
│              ├─ Description
│              ├─ Value Display
│              ├─ Metadata
│              └─ Action Buttons
│                   ├─ Edit Button
│                   └─ History Button
│
├─── History Tab Content
│    └─── Recent Changes Card (IonCard)
│         └─ Change List Items [foreach change]
│              ├─ Key & Timestamp
│              ├─ Old → New Value
│              ├─ Change Reason
│              └─ Changed By
│
├─── Edit Modal (IonModal)
│    ├─ Modal Header
│    ├─ Setting Details
│    ├─ Value Input
│    ├─ Reason Input
│    └─ Save Button
│
├─── Create Modal (IonModal)
│    ├─ Modal Header
│    ├─ Form Fields
│    │    ├─ Key Input
│    │    ├─ Value Input
│    │    ├─ Description Input
│    │    ├─ Data Type Selector
│    │    ├─ Category Selector
│    │    ├─ Public Toggle
│    │    └─ Encrypted Toggle
│    └─ Create Button
│
├─── History Detail Modal (IonModal)
│    ├─ Modal Header
│    └─ History Cards [foreach history entry]
│         ├─ Timestamp
│         ├─ Old → New
│         ├─ Reason
│         └─ Changed By
│
├─── Toast Notifications (IonToast)
│
└─── Confirmation Dialog (ConfirmToast)
```

## State Management

```typescript
// UI State
const [activeTab, setActiveTab] = useState<"settings" | "history">("settings");
const [isLoading, setIsLoading] = useState(false);
const [isSaving, setIsSaving] = useState(false);
const [showToast, setShowToast] = useState(false);

// Data State
const [settings, setSettings] = useState<GlobalSetting[]>([]);
const [filteredSettings, setFilteredSettings] = useState<GlobalSetting[]>([]);
const [recentChanges, setRecentChanges] = useState<GlobalSettingHistory[]>([]);
const [selectedSettingHistory, setSelectedSettingHistory] = useState<GlobalSettingHistory[]>([]);

// Filter State
const [selectedCategory, setSelectedCategory] = useState<string>("all");
const [searchQuery, setSearchQuery] = useState<string>("");

// Modal State
const [showEditModal, setShowEditModal] = useState(false);
const [showCreateModal, setShowCreateModal] = useState(false);
const [showHistoryModal, setShowHistoryModal] = useState(false);
const [selectedSetting, setSelectedSetting] = useState<GlobalSetting | null>(null);

// Form State
const [editValue, setEditValue] = useState<string>("");
const [changeReason, setChangeReason] = useState<string>("");
const [newSetting, setNewSetting] = useState<CreateGlobalSettingRequest>({...});
```

## API Endpoints Matrix

| Method | Endpoint | Purpose | Auth | Response |
|--------|----------|---------|------|----------|
| GET | `/admin/settings` | Get all settings | Admin | `List<GlobalSettingDto>` |
| GET | `/admin/settings/{id}` | Get by ID | Admin | `GlobalSettingDto` |
| GET | `/admin/settings/key/{key}` | Get by key | Admin | `GlobalSettingDto` |
| GET | `/admin/settings/category/{cat}` | Get by category | Admin | `List<GlobalSettingDto>` |
| POST | `/admin/settings/create` | Create new | Admin | `GlobalSettingDto` |
| PUT | `/admin/settings/update` | Update value | Admin | `GlobalSettingDto` |
| POST | `/admin/settings/validate` | Validate value | Admin | `ValidateSettingResponseDto` |
| GET | `/admin/settings/{id}/history` | Get history | Admin | `List<GlobalSettingHistoryDto>` |
| GET | `/admin/settings/changes/recent` | Recent changes | Admin | `List<GlobalSettingHistoryDto>` |

## Security Flow

```
Request
  │
  ├─> API Client adds Authorization header
  │    Authorization: Bearer <JWT_TOKEN>
  │
  ↓
Backend Middleware
  │
  ├─> Validates JWT token
  │
  ├─> Extracts user claims
  │
  ↓
AdminController
  │
  ├─> [Authorize] attribute checks auth
  │
  ├─> Gets userId from User.Claims
  │
  ├─> Calls _adminService.IsAdminAsync(userId)
  │
  ├─> If not admin: return Forbid()
  │
  └─> If admin: proceed with action
  │
  ↓
GlobalSettingsService
  │
  ├─> Records adminUserId in changes
  │
  ├─> Encrypts sensitive values (if isEncrypted)
  │
  └─> Validates permissions (if needed)
```

## Performance Considerations

### Frontend Optimizations
- Settings cached in component state
- Search/filter done client-side
- Debounced search input
- Lazy loading for large lists
- Modal content only rendered when open

### Backend Optimizations
- Indexed columns (key, category, isPublic)
- EF Core query optimization
- Pagination support (future)
- Caching layer (future)

### Database Optimizations
- Unique index on `key` column
- Foreign key indexes
- Proper column types
- Connection pooling

---

**Created**: October 29, 2025  
**Purpose**: Visual reference for Global Settings system architecture  
**Audience**: Developers and system architects

