using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Study_Hub.Service
{
    public class GlobalSettingsService : IGlobalSettingsService
    {
        private readonly ApplicationDbContext _context;

        public GlobalSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CRUD Operations

        public async Task<GlobalSettingDto> CreateSettingAsync(Guid adminUserId, CreateGlobalSettingRequestDto request)
        {
            // Check if key already exists
            var existing = await _context.GlobalSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == request.Key.ToLower());

            if (existing != null)
                throw new InvalidOperationException($"Setting with key '{request.Key}' already exists");

            // Validate value based on data type
            var validation = await ValidateSettingValueAsync(request.Key, request.Value, request.DataType, 
                request.MinValue, request.MaxValue, request.AllowedValues, request.ValidationRegex);

            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage ?? "Invalid setting value");

            var setting = new GlobalSetting
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                DataType = request.DataType,
                Category = request.Category,
                IsPublic = request.IsPublic,
                IsEncrypted = request.IsEncrypted,
                ValidationRegex = request.ValidationRegex,
                DefaultValue = request.DefaultValue,
                MinValue = request.MinValue,
                MaxValue = request.MaxValue,
                AllowedValues = request.AllowedValues,
                UpdatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GlobalSettings.Add(setting);
            await _context.SaveChangesAsync();

            return await MapToDto(setting);
        }

        public async Task<GlobalSettingDto> UpdateSettingAsync(Guid adminUserId, UpdateGlobalSettingRequestDto request)
        {
            var setting = await _context.GlobalSettings.FindAsync(request.Id);
            if (setting == null)
                throw new InvalidOperationException("Setting not found");

            // Validate new value
            var validation = await ValidateSettingValueAsync(setting.Key, request.Value, setting.DataType,
                setting.MinValue, setting.MaxValue, setting.AllowedValues, setting.ValidationRegex);

            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage ?? "Invalid setting value");

            // Record history
            var history = new GlobalSettingHistory
            {
                Id = Guid.NewGuid(),
                SettingId = setting.Id,
                Key = setting.Key,
                OldValue = setting.Value,
                NewValue = request.Value,
                ChangedBy = adminUserId,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = request.ChangeReason
            };

            _context.GlobalSettingsHistory.Add(history);

            // Update setting
            setting.Value = request.Value;
            setting.UpdatedBy = adminUserId;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToDto(setting);
        }

        public async Task<bool> DeleteSettingAsync(Guid settingId)
        {
            var setting = await _context.GlobalSettings.FindAsync(settingId);
            if (setting == null)
                return false;

            _context.GlobalSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GlobalSettingDto?> GetSettingByIdAsync(Guid settingId)
        {
            var setting = await _context.GlobalSettings
                .Include(s => s.UpdatedByUser)
                .FirstOrDefaultAsync(s => s.Id == settingId);

            if (setting == null)
                return null;

            return await MapToDto(setting);
        }

        public async Task<GlobalSettingDto?> GetSettingByKeyAsync(string key)
        {
            var setting = await _context.GlobalSettings
                .Include(s => s.UpdatedByUser)
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower());

            if (setting == null)
                return null;

            return await MapToDto(setting);
        }

        public async Task<List<GlobalSettingDto>> GetAllSettingsAsync()
        {
            var settings = await _context.GlobalSettings
                .Include(s => s.UpdatedByUser)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToListAsync();

            var dtos = new List<GlobalSettingDto>();
            foreach (var setting in settings)
            {
                dtos.Add(await MapToDto(setting));
            }

            return dtos;
        }

        public async Task<List<GlobalSettingDto>> GetSettingsByCategoryAsync(string category)
        {
            var settings = await _context.GlobalSettings
                .Include(s => s.UpdatedByUser)
                .Where(s => s.Category.ToLower() == category.ToLower())
                .OrderBy(s => s.Key)
                .ToListAsync();

            var dtos = new List<GlobalSettingDto>();
            foreach (var setting in settings)
            {
                dtos.Add(await MapToDto(setting));
            }

            return dtos;
        }

        public async Task<List<GlobalSettingDto>> GetPublicSettingsAsync()
        {
            var settings = await _context.GlobalSettings
                .Where(s => s.IsPublic)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToListAsync();

            var dtos = new List<GlobalSettingDto>();
            foreach (var setting in settings)
            {
                dtos.Add(await MapToDto(setting));
            }

            return dtos;
        }

        // Bulk Operations

        public async Task<Dictionary<string, GlobalSettingDto>> BulkUpdateSettingsAsync(Guid adminUserId, BulkUpdateSettingsRequestDto request)
        {
            var result = new Dictionary<string, GlobalSettingDto>();

            foreach (var kvp in request.Settings)
            {
                var setting = await _context.GlobalSettings
                    .FirstOrDefaultAsync(s => s.Key.ToLower() == kvp.Key.ToLower());

                if (setting == null)
                    continue;

                // Validate
                var validation = await ValidateSettingValueAsync(setting.Key, kvp.Value, setting.DataType,
                    setting.MinValue, setting.MaxValue, setting.AllowedValues, setting.ValidationRegex);

                if (!validation.IsValid)
                    continue;

                // Record history
                var history = new GlobalSettingHistory
                {
                    Id = Guid.NewGuid(),
                    SettingId = setting.Id,
                    Key = setting.Key,
                    OldValue = setting.Value,
                    NewValue = kvp.Value,
                    ChangedBy = adminUserId,
                    ChangedAt = DateTime.UtcNow,
                    ChangeReason = request.ChangeReason
                };

                _context.GlobalSettingsHistory.Add(history);

                // Update
                setting.Value = kvp.Value;
                setting.UpdatedBy = adminUserId;
                setting.UpdatedAt = DateTime.UtcNow;

                result[kvp.Key] = await MapToDto(setting);
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<bool> InitializeDefaultSettingsAsync(Guid adminUserId, bool overwriteExisting = false)
        {
            var defaultSettings = GetDefaultSettings();

            foreach (var setting in defaultSettings)
            {
                var existing = await _context.GlobalSettings
                    .FirstOrDefaultAsync(s => s.Key == setting.Key);

                if (existing != null && !overwriteExisting)
                    continue;

                if (existing != null)
                {
                    // Update existing
                    var history = new GlobalSettingHistory
                    {
                        Id = Guid.NewGuid(),
                        SettingId = existing.Id,
                        Key = existing.Key,
                        OldValue = existing.Value,
                        NewValue = setting.Value,
                        ChangedBy = adminUserId,
                        ChangedAt = DateTime.UtcNow,
                        ChangeReason = "Default settings initialization"
                    };

                    _context.GlobalSettingsHistory.Add(history);

                    existing.Value = setting.Value;
                    existing.UpdatedBy = adminUserId;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new
                    setting.Id = Guid.NewGuid();
                    setting.UpdatedBy = adminUserId;
                    setting.CreatedAt = DateTime.UtcNow;
                    setting.UpdatedAt = DateTime.UtcNow;
                    _context.GlobalSettings.Add(setting);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // History

        public async Task<List<GlobalSettingHistoryDto>> GetSettingHistoryAsync(Guid settingId)
        {
            var history = await _context.GlobalSettingsHistory
                .Include(h => h.ChangedByUser)
                .Where(h => h.SettingId == settingId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return history.Select(h => new GlobalSettingHistoryDto
            {
                Id = h.Id,
                SettingId = h.SettingId,
                Key = h.Key,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ChangedBy = h.ChangedBy,
                ChangedByEmail = h.ChangedByUser?.Email ?? "Unknown",
                ChangedAt = h.ChangedAt,
                ChangeReason = h.ChangeReason
            }).ToList();
        }

        public async Task<List<GlobalSettingHistoryDto>> GetRecentChangesAsync(int count = 50)
        {
            var history = await _context.GlobalSettingsHistory
                .Include(h => h.ChangedByUser)
                .OrderByDescending(h => h.ChangedAt)
                .Take(count)
                .ToListAsync();

            return history.Select(h => new GlobalSettingHistoryDto
            {
                Id = h.Id,
                SettingId = h.SettingId,
                Key = h.Key,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ChangedBy = h.ChangedBy,
                ChangedByEmail = h.ChangedByUser?.Email ?? "Unknown",
                ChangedAt = h.ChangedAt,
                ChangeReason = h.ChangeReason
            }).ToList();
        }

        // Validation

        public async Task<ValidateSettingResponseDto> ValidateSettingAsync(ValidateSettingRequestDto request)
        {
            var setting = await _context.GlobalSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == request.Key.ToLower());

            if (setting == null)
            {
                return new ValidateSettingResponseDto
                {
                    IsValid = false,
                    ErrorMessage = "Setting not found"
                };
            }

            return await ValidateSettingValueAsync(setting.Key, request.Value, setting.DataType,
                setting.MinValue, setting.MaxValue, setting.AllowedValues, setting.ValidationRegex);
        }

        // Import/Export

        public async Task<ExportSettingsResponseDto> ExportSettingsAsync(string? category = null)
        {
            var query = _context.GlobalSettings.Include(s => s.UpdatedByUser).AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.Category.ToLower() == category.ToLower());
            }

            var settings = await query.ToListAsync();
            var settingsDict = new Dictionary<string, GlobalSettingDto>();

            foreach (var setting in settings)
            {
                settingsDict[setting.Key] = await MapToDto(setting);
            }

            return new ExportSettingsResponseDto
            {
                Settings = settingsDict,
                ExportedAt = DateTime.UtcNow,
                ExportedBy = "Admin" // Could be enhanced to include actual user
            };
        }

        public async Task<Dictionary<string, GlobalSettingDto>> ImportSettingsAsync(Guid adminUserId, ImportSettingsRequestDto request)
        {
            var result = new Dictionary<string, GlobalSettingDto>();

            foreach (var kvp in request.Settings)
            {
                var existing = await _context.GlobalSettings
                    .FirstOrDefaultAsync(s => s.Key.ToLower() == kvp.Key.ToLower());

                if (existing != null)
                {
                    if (!request.OverwriteExisting)
                        continue;

                    // Update existing
                    var history = new GlobalSettingHistory
                    {
                        Id = Guid.NewGuid(),
                        SettingId = existing.Id,
                        Key = existing.Key,
                        OldValue = existing.Value,
                        NewValue = kvp.Value,
                        ChangedBy = adminUserId,
                        ChangedAt = DateTime.UtcNow,
                        ChangeReason = request.ImportReason ?? "Settings import"
                    };

                    _context.GlobalSettingsHistory.Add(history);

                    existing.Value = kvp.Value;
                    existing.UpdatedBy = adminUserId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    result[kvp.Key] = await MapToDto(existing);
                }
                else
                {
                    // Create new with default metadata
                    var newSetting = new GlobalSetting
                    {
                        Id = Guid.NewGuid(),
                        Key = kvp.Key,
                        Value = kvp.Value,
                        DataType = "string",
                        Category = "imported",
                        IsPublic = false,
                        UpdatedBy = adminUserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.GlobalSettings.Add(newSetting);
                    result[kvp.Key] = await MapToDto(newSetting);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        // Summary & Analytics

        public async Task<SettingsSummaryDto> GetSettingsSummaryAsync()
        {
            var settings = await _context.GlobalSettings.ToListAsync();

            var byCategory = settings
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var lastUpdated = await _context.GlobalSettings
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();

            var lastUpdatedByUser = lastUpdated != null
                ? await _context.Users.FindAsync(lastUpdated.UpdatedBy)
                : null;

            return new SettingsSummaryDto
            {
                TotalSettings = settings.Count,
                ByCategory = byCategory,
                PublicSettings = settings.Count(s => s.IsPublic),
                EncryptedSettings = settings.Count(s => s.IsEncrypted),
                LastUpdated = lastUpdated?.UpdatedAt ?? DateTime.MinValue,
                LastUpdatedBy = lastUpdatedByUser?.Email
            };
        }

        // Helper Methods

        public async Task<string?> GetSettingValueAsync(string key)
        {
            var setting = await _context.GlobalSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower());

            return setting?.Value;
        }

        public async Task<T?> GetSettingValueAsync<T>(string key)
        {
            var value = await GetSettingValueAsync(key);
            if (value == null)
                return default;

            try
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(value);

                if (typeof(T) == typeof(decimal))
                    return (T)(object)decimal.Parse(value);

                if (typeof(T) == typeof(bool))
                    return (T)(object)bool.Parse(value);

                // For complex types, try JSON deserialization
                return JsonSerializer.Deserialize<T>(value);
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> SetSettingValueAsync(Guid adminUserId, string key, string value, string? changeReason = null)
        {
            var setting = await _context.GlobalSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower());

            if (setting == null)
                return false;

            // Validate
            var validation = await ValidateSettingValueAsync(setting.Key, value, setting.DataType,
                setting.MinValue, setting.MaxValue, setting.AllowedValues, setting.ValidationRegex);

            if (!validation.IsValid)
                return false;

            // Record history
            var history = new GlobalSettingHistory
            {
                Id = Guid.NewGuid(),
                SettingId = setting.Id,
                Key = setting.Key,
                OldValue = setting.Value,
                NewValue = value,
                ChangedBy = adminUserId,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = changeReason
            };

            _context.GlobalSettingsHistory.Add(history);

            // Update
            setting.Value = value;
            setting.UpdatedBy = adminUserId;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Private Helper Methods

        private async Task<GlobalSettingDto> MapToDto(GlobalSetting setting)
        {
            if (setting.UpdatedByUser == null && setting.UpdatedBy.HasValue)
            {
                await _context.Entry(setting).Reference(s => s.UpdatedByUser).LoadAsync();
            }

            return new GlobalSettingDto
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                DataType = setting.DataType,
                Category = setting.Category,
                IsPublic = setting.IsPublic,
                IsEncrypted = setting.IsEncrypted,
                ValidationRegex = setting.ValidationRegex,
                DefaultValue = setting.DefaultValue,
                MinValue = setting.MinValue,
                MaxValue = setting.MaxValue,
                AllowedValues = setting.AllowedValues,
                UpdatedBy = setting.UpdatedBy,
                UpdatedByEmail = setting.UpdatedByUser?.Email,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
        }

        private async Task<ValidateSettingResponseDto> ValidateSettingValueAsync(
            string key, string value, string dataType, decimal? minValue, decimal? maxValue,
            string? allowedValues, string? validationRegex)
        {
            // Type validation
            switch (dataType.ToLower())
            {
                case "number":
                case "decimal":
                    if (!decimal.TryParse(value, out var numValue))
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = "Value must be a valid number"
                        };
                    }

                    if (minValue.HasValue && numValue < minValue.Value)
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = $"Value must be at least {minValue.Value}"
                        };
                    }

                    if (maxValue.HasValue && numValue > maxValue.Value)
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = $"Value must be at most {maxValue.Value}"
                        };
                    }
                    break;

                case "boolean":
                case "bool":
                    if (!bool.TryParse(value, out _))
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = "Value must be 'true' or 'false'"
                        };
                    }
                    break;

                case "json":
                    try
                    {
                        JsonDocument.Parse(value);
                    }
                    catch
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = "Value must be valid JSON"
                        };
                    }
                    break;
            }

            // Allowed values validation
            if (!string.IsNullOrEmpty(allowedValues))
            {
                try
                {
                    var allowed = JsonSerializer.Deserialize<List<string>>(allowedValues);
                    if (allowed != null && !allowed.Contains(value))
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = $"Value must be one of: {string.Join(", ", allowed)}"
                        };
                    }
                }
                catch { }
            }

            // Regex validation
            if (!string.IsNullOrEmpty(validationRegex))
            {
                try
                {
                    if (!Regex.IsMatch(value, validationRegex))
                    {
                        return new ValidateSettingResponseDto
                        {
                            IsValid = false,
                            ErrorMessage = "Value does not match required format"
                        };
                    }
                }
                catch { }
            }

            return new ValidateSettingResponseDto
            {
                IsValid = true
            };
        }

        private List<GlobalSetting> GetDefaultSettings()
        {
            return new List<GlobalSetting>
            {
                // System Settings
                new GlobalSetting
                {
                    Key = "system.maintenance_mode",
                    Value = "false",
                    Description = "Enable maintenance mode to prevent user access",
                    DataType = "boolean",
                    Category = "system",
                    IsPublic = true
                },
                new GlobalSetting
                {
                    Key = "system.app_name",
                    Value = "Study Hub",
                    Description = "Application name displayed to users",
                    DataType = "string",
                    Category = "system",
                    IsPublic = true
                },
                new GlobalSetting
                {
                    Key = "system.max_upload_size_mb",
                    Value = "10",
                    Description = "Maximum file upload size in MB",
                    DataType = "number",
                    Category = "system",
                    MinValue = 1,
                    MaxValue = 100
                },

                // Credit Settings
                new GlobalSetting
                {
                    Key = "credits.default_package_amount",
                    Value = "100",
                    Description = "Default credit package amount",
                    DataType = "number",
                    Category = "credits",
                    MinValue = 1
                },
                new GlobalSetting
                {
                    Key = "credits.min_purchase_amount",
                    Value = "50",
                    Description = "Minimum credits that can be purchased",
                    DataType = "number",
                    Category = "credits",
                    MinValue = 1
                },

                // Notification Settings
                new GlobalSetting
                {
                    Key = "notifications.enabled",
                    Value = "true",
                    Description = "Enable push notifications",
                    DataType = "boolean",
                    Category = "notifications"
                },
                new GlobalSetting
                {
                    Key = "notifications.email_enabled",
                    Value = "true",
                    Description = "Enable email notifications",
                    DataType = "boolean",
                    Category = "notifications"
                },

                // Table Settings
                new GlobalSetting
                {
                    Key = "tables.default_hourly_rate",
                    Value = "50",
                    Description = "Default hourly rate for study tables",
                    DataType = "number",
                    Category = "tables",
                    MinValue = 1
                },
                new GlobalSetting
                {
                    Key = "tables.max_booking_hours",
                    Value = "8",
                    Description = "Maximum hours a table can be booked",
                    DataType = "number",
                    Category = "tables",
                    MinValue = 1,
                    MaxValue = 24
                }
            };
        }
    }
}
