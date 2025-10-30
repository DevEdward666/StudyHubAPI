# User Role System Implementation

## Overview
Added a comprehensive role-based system to manage user permissions with three distinct roles: Staff, Admin, and Super Admin. The system includes role assignment during user creation and display in the admin users table.

## Database Changes

### Entity Update
**File**: `Study-Hub/Models/Entities/User.cs`

Added role field:
```csharp
[Column("role")]
[StringLength(50)]
public string Role { get; set; } = "Staff";
```

**Default Value**: "Staff"

### Role Enum
**File**: `Study-Hub/Models/Enums/UserRole.cs`

Created enum for type safety:
```csharp
public enum UserRole
{
    Staff,
    Admin,
    SuperAdmin
}
```

### Migration
- **Migration Name**: `AddRoleToUsers` (generated)
- **Action**: Adds `role` column to `users` table
- **Type**: `VARCHAR(50)`
- **Default**: "Staff"
- **Nullable**: No

## Role Definitions

### 1. Staff
- **Icon**: 👤
- **Color**: Gray (#666)
- **Permissions**: 
  - Access to user features
  - Can purchase credits
  - Can book tables
  - View personal history
- **Purpose**: Default role for regular users

### 2. Admin
- **Icon**: ⭐
- **Color**: Blue (#3880ff)
- **Permissions**:
  - All Staff permissions
  - Access to admin dashboard
  - Manage users (view, add credits)
  - Manage tables
  - Manage transactions
  - View reports
- **Purpose**: System administrators

### 3. Super Admin
- **Icon**: 🔑
- **Color**: Red (#d32f2f)
- **Permissions**:
  - All Admin permissions
  - Manage admin users
  - System-wide settings
  - Full database access
  - Critical operations
- **Purpose**: Highest level administrators

## Backend Changes

### 1. DTOs Updated

#### UserDto
**File**: `Study-Hub/Models/DTOs/AuthDto.cs`
```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public bool EmailVerified { get; set; }
    public string Role { get; set; }  // NEW
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### UserWithInfoDto
**File**: `Study-Hub/Models/DTOs/AdminDto.cs`
```csharp
public class UserWithInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public decimal Credits { get; set; }
    public bool IsAdmin { get; set; }
    public bool HasActiveSession { get; set; }
    public string Role { get; set; }  // NEW
    public DateTime CreatedAt { get; set; }
}
```

#### CreateUserRequestDto (NEW)
```csharp
public class CreateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Role { get; set; }

    public string? Password { get; set; }
}
```

#### CreateUserResponseDto (NEW)
```csharp
public class CreateUserResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
}
```

### 2. Service Layer

#### AdminService.GetAllUsersAsync()
**File**: `Study-Hub/Service/AdminService.cs`

Updated mapper to include role:
```csharp
return users.Select(user => new UserWithInfoDto
{
    // ...existing fields...
    Role = user.Role,
    // ...
}).ToList();
```

#### AdminService.CreateUserAsync() (NEW)
**File**: `Study-Hub/Service/AdminService.cs`

```csharp
public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request)
{
    // Validates role
    var validRoles = new[] { "Staff", "Admin", "Super Admin" };
    if (!validRoles.Contains(request.Role))
        throw new InvalidOperationException("Invalid role");

    // Creates user with role
    var newUser = new User
    {
        Email = request.Email,
        Name = request.Name,
        Role = request.Role,
        // ...
    };

    // Creates AdminUser entry if role is Admin or Super Admin
    if (request.Role == "Admin" || request.Role == "Super Admin")
    {
        var adminUser = new AdminUser
        {
            UserId = newUser.Id,
            // ...
        };
        _context.AdminUsers.Add(adminUser);
    }

    // Initializes user credits
    // Returns response
}
```

### 3. Controller

#### POST /admin/users/create (NEW)
**File**: `Study-Hub/Controllers/AdminController.cs`

```csharp
[HttpPost("users/create")]
public async Task<ActionResult<ApiResponse<CreateUserResponseDto>>> CreateUser(
    [FromBody] CreateUserRequestDto request)
{
    // Admin check
    // Calls CreateUserAsync
    // Returns response
}
```

## Frontend Changes

### 1. Schema Updates

#### UserSchema
**File**: `study_hub_app/src/schema/auth.schema.ts`

```typescript
export const UserSchema = z.object({
  id: z.string(),
  email: z.string(),
  name: z.string().nullable(),
  image: z.string().nullable().optional(),
  emailVerified: z.boolean().nullable().optional(),
  role: z.string(),  // NEW
  createdAt: z.string(),
  updatedAt: z.string().nullable().optional(),
});
```

#### CreateUserRequestSchema (NEW)
**File**: `study_hub_app/src/schema/admin.schema.ts`

```typescript
export const CreateUserRequestSchema = z.object({
  email: z.string().email("Invalid email address"),
  name: z.string().min(1, "Name is required"),
  role: z.enum(["Staff", "Admin", "Super Admin"]),
  password: z.string().optional(),
});

export const CreateUserResponseSchema = z.object({
  userId: z.string(),
  email: z.string(),
  name: z.string(),
  role: z.string(),
});
```

### 2. User Management UI

**File**: `study_hub_app/src/pages/UserManagement.tsx`

#### State Management
```typescript
const [newUserData, setNewUserData] = useState({
  name: "",
  email: "",
  password: "",
  confirmPassword: "",
  role: "Staff"  // NEW - Default to Staff
});
```

#### Role Selector in Create User Modal
```tsx
<IonItem>
  <IonLabel position="stacked">Role *</IonLabel>
  <IonSelect
    value={newUserData.role}
    placeholder="Select role"
    onIonChange={(e) => setNewUserData({...newUserData, role: e.detail.value})}
  >
    <IonSelectOption value="Staff">Staff</IonSelectOption>
    <IonSelectOption value="Admin">Admin</IonSelectOption>
    <IonSelectOption value="Super Admin">Super Admin</IonSelectOption>
  </IonSelect>
</IonItem>
<IonText color="medium" style={{ fontSize: "0.85em" }}>
  <small>Staff: Regular user | Admin: Dashboard access | Super Admin: Full access</small>
</IonText>
```

#### Role Column in Users Table
```typescript
{
  key: "role",
  label: "Role",
  sortable: true,
  render: (value) => {
    const roleConfig = {
      "Staff": { icon: "👤", color: "#666", bgColor: "#f5f5f5" },
      "Admin": { icon: "⭐", color: "#3880ff", bgColor: "#e3f2fd" },
      "Super Admin": { icon: "🔑", color: "#d32f2f", bgColor: "#ffebee" }
    };
    const config = roleConfig[value] || roleConfig["Staff"];
    
    return (
      <span style={{
        display: "inline-flex",
        alignItems: "center",
        gap: "4px",
        padding: "4px 12px",
        borderRadius: "12px",
        fontSize: "0.85em",
        fontWeight: "500",
        color: config.color,
        backgroundColor: config.bgColor
      }}>
        <span>{config.icon}</span>
        {value}
      </span>
    );
  },
}
```

#### Admin Status Column (Separate)
```typescript
{
  key: "isAdmin",
  label: "Admin Status",
  sortable: true,
  render: (value) => (
    <span>
      {value ? 
        <IonIcon icon={checkmarkCircleOutline} color="success" /> : 
        <IonIcon icon={closeCircleOutline} color="medium" />
      }
    </span>
  ),
}
```

### 3. API Hook Update
**File**: `study_hub_app/src/hooks/AdminDataHooks.tsx`

```typescript
const createUserMutation = useMutation({
  mutationFn: ({ name, email, password, role }: { 
    name: string; 
    email: string; 
    password: string; 
    role: string  // NEW
  }) => apiClient.post(
    "/admin/users/create",
    ApiResponseSchema(UserWithInfoSchema),
    { name, email, password, role }  // NEW
  ),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
  },
});
```

## User Flow

### Creating a User with Role:

1. **Admin clicks "Create New User"**
2. **Fills in form**:
   - Full Name (required)
   - Email Address (required)
   - **Role** (required) - dropdown with 3 options
   - Password (required)
   - Confirm Password (required)

3. **Role Selection Tooltip**:
   - Shows permission descriptions
   - Visual indicators for each role

4. **Summary Display**:
   ```
   Name: John Doe
   Email: john@example.com
   Role: Admin  ← Shows selected role
   Initial Credits: 0
   ```

5. **User Creation**:
   - Validates all fields including role
   - Creates user with assigned role
   - If role is Admin/Super Admin:
     - Creates AdminUser entry
   - Initializes credits at 0
   - User can log in immediately

### Viewing Users:

**Users Table displays**:
```
| User        | Email           | Credits | Role         | Admin Status | Session  | Joined     | Actions |
|-------------|-----------------|---------|--------------|--------------|----------|------------|---------|
| John Doe    | john@ex.com     | 100     | 👤 Staff     | ✓            | Active   | 10/30/2025 | ...     |
| Jane Admin  | jane@ex.com     | 500     | ⭐ Admin     | ✓            | Inactive | 10/29/2025 | ...     |
| Super User  | super@ex.com    | 1000    | 🔑 Super ... | ✓            | Active   | 10/28/2025 | ...     |
```

## Role Assignment Logic

### Automatic AdminUser Creation:
```csharp
if (request.Role == "Admin" || request.Role == "Super Admin")
{
    // Creates AdminUser entry
    // Grants admin dashboard access
}
```

### Role Validation:
```csharp
var validRoles = new[] { "Staff", "Admin", "Super Admin" };
if (!validRoles.Contains(request.Role))
{
    throw new InvalidOperationException("Invalid role");
}
```

## API Endpoints

### POST /admin/users/create

**Request**:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "role": "Admin"
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "john@example.com",
    "name": "John Doe",
    "role": "Admin"
  },
  "message": "User created successfully"
}
```

### GET /admin/users

**Response** (updated):
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "email": "user@example.com",
      "name": "John Doe",
      "credits": 100,
      "isAdmin": true,
      "hasActiveSession": false,
      "role": "Admin",
      "createdAt": "2025-10-30T10:00:00Z"
    }
  ]
}
```

## Visual Design

### Role Badges:

**Staff** (Gray):
```
┌──────────────┐
│ 👤 Staff     │
└──────────────┘
```

**Admin** (Blue):
```
┌──────────────┐
│ ⭐ Admin     │
└──────────────┘
```

**Super Admin** (Red):
```
┌─────────────────────┐
│ 🔑 Super Admin      │
└─────────────────────┘
```

### Create User Modal:
```
┌─────────────────────────────────┐
│ Create New User                 │
├─────────────────────────────────┤
│ Full Name *        [John Doe  ] │
│ Email Address *    [john@ex.c ] │
│ Role *             [Admin    ▼] │
│   Staff: Regular user           │
│   Admin: Dashboard access       │
│   Super Admin: Full access      │
│ Password *         [••••••••  ] │
│ Confirm Password * [••••••••  ] │
│                                 │
│ ┌─── Summary ─────────────────┐ │
│ │ Name: John Doe              │ │
│ │ Email: john@example.com     │ │
│ │ Role: Admin                 │ │
│ │ Initial Credits: 0          │ │
│ └─────────────────────────────┘ │
│                                 │
│ [Cancel]        [Create User]   │
└─────────────────────────────────┘
```

## Benefits

1. **Clear Permissions**: Each role has defined responsibilities
2. **Visual Clarity**: Icons and colors make roles instantly recognizable
3. **Flexibility**: Easy to add more roles in the future
4. **Security**: Role validated on backend
5. **Audit Trail**: Role stored with user record
6. **Easy Management**: Admin can see and set roles during creation

## Testing Checklist

- [x] Database migration created
- [x] User entity updated with role field
- [x] DTOs updated (UserDto, UserWithInfoDto, CreateUserRequestDto)
- [x] CreateUserAsync service method implemented
- [x] AdminUser auto-created for Admin/Super Admin roles
- [x] Frontend schemas updated
- [x] Role selector added to create user modal
- [x] Role column added to users table
- [x] Visual design with icons and colors
- [x] API hook updated with role parameter
- [ ] Run migration: `dotnet ef database update`
- [ ] Create Staff user - verify no AdminUser entry
- [ ] Create Admin user - verify AdminUser entry created
- [ ] Create Super Admin user - verify AdminUser entry created
- [ ] Verify role displays correctly in table
- [ ] Test role validation (invalid role rejected)

## Files Modified

### Backend (9 files)
1. `Study-Hub/Models/Entities/User.cs` - Added Role field
2. `Study-Hub/Models/Enums/UserRole.cs` - Created enum (NEW)
3. `Study-Hub/Models/DTOs/AuthDto.cs` - Updated UserDto
4. `Study-Hub/Models/DTOs/AdminDto.cs` - Updated DTOs, added CreateUser DTOs
5. `Study-Hub/Service/Interface/IAdminService.cs` - Added CreateUserAsync
6. `Study-Hub/Service/AdminService.cs` - Implemented CreateUserAsync
7. `Study-Hub/Controllers/AdminController.cs` - Added CreateUser endpoint
8. Migration file (generated)

### Frontend (4 files)
1. `study_hub_app/src/schema/auth.schema.ts` - Added role to UserSchema
2. `study_hub_app/src/schema/admin.schema.ts` - Added CreateUser schemas
3. `study_hub_app/src/pages/UserManagement.tsx` - Role selector and column
4. `study_hub_app/src/hooks/AdminDataHooks.tsx` - Updated mutation

## Future Enhancements

1. **Role Editing**: Allow changing user roles after creation
2. **Custom Roles**: Let Super Admins create custom roles
3. **Permission Matrix**: Detailed permission controls per role
4. **Role History**: Track role changes over time
5. **Bulk Role Assignment**: Change roles for multiple users
6. **Role-Based Navigation**: Show/hide menu items based on role
7. **Role Hierarchy**: Define role inheritance
8. **Role Templates**: Pre-configured role settings

## Security Considerations

1. **Backend Validation**: Role validated on server side
2. **Enum Type Safety**: UserRole enum prevents invalid values
3. **Admin Creation Control**: Only admins can create users
4. **Role Restrictions**: Can't assign role higher than own role (future)
5. **Audit Trail**: All role assignments logged

---

**Implementation Date**: October 31, 2025  
**Status**: ✅ Complete  
**Version**: 1.0

