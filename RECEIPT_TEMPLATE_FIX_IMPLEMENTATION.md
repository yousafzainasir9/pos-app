# 🔧 Receipt Template Persistence - Implementation Fix

## Files to Update

Apply these changes to fix the receipt template persistence issue.

---

## File 1: `frontend/src/pages/SystemSettingsPage.tsx`

### Changes Required:

1. **Add enhanced debugging to `loadSettings` function** (around line 100)
2. **Reload settings after save to confirm persistence**
3. **Add key prop to force component re-render**

### Updated Code Sections:

#### Section A: Enhanced loadSettings Function

**Replace lines 100-122 with:**

```typescript
const loadSettings = async () => {
  try {
    setLoading(true);
    const [receipt, email, defaults] = await Promise.all([
      systemSettingsService.getReceiptSettings(),
      systemSettingsService.getEmailSettings(),
      systemSettingsService.getDefaultValues()
    ]);

    // 🔍 DEBUG: Log what we received from backend
    console.log('📥 [SystemSettings] Loaded from backend:', {
      receiptTemplate: receipt.receiptTemplate,
      paperSize: receipt.paperSize,
      fontSize: receipt.fontSize,
      timestamp: new Date().toISOString()
    });

    // Properly merge loaded settings with defaults, prioritizing loaded values
    setReceiptSettings(prev => {
      const newSettings = {
        ...prev,
        ...receipt,
        // Ensure critical fields are set
        receiptTemplate: receipt.receiptTemplate || prev.receiptTemplate,
        paperSize: receipt.paperSize || prev.paperSize,
        fontSize: receipt.fontSize || prev.fontSize
      };
      
      // 🔍 DEBUG: Log the merged state
      console.log('✅ [SystemSettings] Merged state:', {
        receiptTemplate: newSettings.receiptTemplate,
        paperSize: newSettings.paperSize,
        fontSize: newSettings.fontSize
      });
      
      return newSettings;
    });
    
    setEmailSettings(prev => ({ ...prev, ...email }));
    setDefaultValues(prev => ({ ...prev, ...defaults }));
  } catch (error) {
    console.error('❌ [SystemSettings] Failed to load:', error);
    toast.error('Failed to load settings');
  } finally {
    setLoading(false);
  }
};
```

#### Section B: Enhanced handleSaveReceipt Function

**Replace lines 124-137 with:**

```typescript
const handleSaveReceipt = async () => {
  try {
    setLoading(true);
    
    // 🔍 DEBUG: Log what we're saving
    console.log('💾 [SystemSettings] Saving receipt settings:', {
      receiptTemplate: receiptSettings.receiptTemplate,
      paperSize: receiptSettings.paperSize,
      fontSize: receiptSettings.fontSize,
      fullSettings: receiptSettings
    });
    
    await systemSettingsService.updateReceiptSettings(receiptSettings);
    clearSettingsCache(); // Clear cache so new settings take effect immediately
    
    // 🔍 DEBUG: Verify save by reloading
    console.log('🔄 [SystemSettings] Reloading to verify save...');
    await loadSettings();
    
    toast.success('Receipt settings saved successfully');
  } catch (error: any) {
    console.error('❌ [SystemSettings] Save failed:', error);
    toast.error(error.response?.data?.message || 'Failed to save receipt settings');
  } finally {
    setLoading(false);
  }
};
```

#### Section C: Add Key Prop to EnhancedReceiptSettings

**Find line ~289 (inside Receipt Settings tab) and update:**

```typescript
{/* Receipt Settings - Using Enhanced Component */}
<Tab.Pane eventKey="receipt">
  <EnhancedReceiptSettings 
    key={`receipt-${receiptSettings.receiptTemplate}-${receiptSettings.paperSize}`}
    initialSettings={receiptSettings}
    onSave={handleSaveReceipt}
  />
</Tab.Pane>
```

---

## File 2: `frontend/src/components/EnhancedReceiptSettings.tsx`

### Changes Required:

1. **Enhanced useEffect with better dependency tracking**
2. **Add debugging logs**
3. **Ensure state updates correctly**

### Updated Code Sections:

#### Section A: Enhanced useState Initialization

**Replace lines 195-203 with:**

```typescript
const [settings, setSettings] = useState<ReceiptSettings>(() => {
  const initial = {
    ...defaultSettings,
    ...(initialSettings || {})
  };
  
  console.log('🎬 [EnhancedReceipt] Initial state:', {
    receiptTemplate: initial.receiptTemplate,
    paperSize: initial.paperSize,
    fontSize: initial.fontSize
  });
  
  return initial;
});
```

#### Section B: Enhanced useEffect

**Replace lines 207-215 with:**

```typescript
// Update settings when initialSettings change (e.g., after loading from backend)
useEffect(() => {
  if (initialSettings && initialSettings.receiptTemplate) {
    console.log('📥 [EnhancedReceipt] Received initialSettings:', {
      receiptTemplate: initialSettings.receiptTemplate,
      paperSize: initialSettings.paperSize,
      fontSize: initialSettings.fontSize,
      timestamp: new Date().toISOString()
    });
    
    setSettings(prev => {
      const newSettings = {
        ...prev,
        ...initialSettings
      };
      
      console.log('✅ [EnhancedReceipt] Updated local state:', {
        receiptTemplate: newSettings.receiptTemplate,
        paperSize: newSettings.paperSize,
        fontSize: newSettings.fontSize
      });
      
      return newSettings;
    });
  } else {
    console.log('⚠️ [EnhancedReceipt] No initialSettings or receiptTemplate missing');
  }
}, [initialSettings, initialSettings?.receiptTemplate]); // Watch both object and specific field
```

---

## File 3: `frontend/src/services/systemSettings.service.ts`

### Changes Required:

Add cache-busting headers to prevent stale responses.

### Updated Code Sections:

**Replace the `getReceiptSettings` function (around line 91) with:**

```typescript
// Receipt Settings
getReceiptSettings: async (): Promise<ReceiptSettingsDto> => {
  const response = await api.get('/systemsettings/receipt', {
    headers: {
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0'
    }
  });
  
  console.log('🌐 [API] Receipt settings response:', {
    receiptTemplate: response.data.data?.receiptTemplate,
    paperSize: response.data.data?.paperSize,
    fontSize: response.data.data?.fontSize
  });
  
  return response.data.data;
},
```

---

## Testing Instructions

After implementing these changes:

### Step 1: Clear Everything

```bash
# Clear browser cache
# In browser: Ctrl+Shift+Delete → Clear cached images and files

# If using Redis or any cache, clear it
# Restart backend if needed
```

### Step 2: Open Browser DevTools

1. Press F12
2. Go to Console tab
3. Clear console (Ctrl+L)

### Step 3: Test Sequence

```
1. Navigate to /admin/settings
   Expected Console:
   🎬 [EnhancedReceipt] Initial state: { receiptTemplate: 'standard', ... }
   🌐 [API] Receipt settings response: { receiptTemplate: 'standard', ... }
   📥 [SystemSettings] Loaded from backend: { receiptTemplate: 'standard', ... }
   ✅ [SystemSettings] Merged state: { receiptTemplate: 'standard', ... }
   📥 [EnhancedReceipt] Received initialSettings: { receiptTemplate: 'standard', ... }
   ✅ [EnhancedReceipt] Updated local state: { receiptTemplate: 'standard', ... }

2. Click "Modern" template
   UI: Modern template should have blue border + "✓ Selected" badge

3. Click "Save Receipt Settings"
   Expected Console:
   💾 [SystemSettings] Saving receipt settings: { receiptTemplate: 'modern', ... }
   🔄 [SystemSettings] Reloading to verify save...
   🌐 [API] Receipt settings response: { receiptTemplate: 'modern', ... }
   📥 [SystemSettings] Loaded from backend: { receiptTemplate: 'modern', ... }
   ✅ [SystemSettings] Merged state: { receiptTemplate: 'modern', ... }
   📥 [EnhancedReceipt] Received initialSettings: { receiptTemplate: 'modern', ... }
   ✅ [EnhancedReceipt] Updated local state: { receiptTemplate: 'modern', ... }
   
   UI: Modern template still selected
   Toast: "Receipt settings saved successfully"

4. Press F5 to refresh
   Expected Console: (same as step 1, but with 'modern')
   🎬 [EnhancedReceipt] Initial state: { receiptTemplate: 'standard', ... }
   🌐 [API] Receipt settings response: { receiptTemplate: 'modern', ... }
   📥 [SystemSettings] Loaded from backend: { receiptTemplate: 'modern', ... }
   ✅ [SystemSettings] Merged state: { receiptTemplate: 'modern', ... }
   📥 [EnhancedReceipt] Received initialSettings: { receiptTemplate: 'modern', ... }
   ✅ [EnhancedReceipt] Updated local state: { receiptTemplate: 'modern', ... }
   
   UI: Modern template selected ✅
```

### Step 4: Database Verification

Run this SQL query:

```sql
SELECT * FROM SystemSettings 
WHERE [Key] = 'Receipt.ReceiptTemplate';
```

**Expected Result:**
```
Key                         | Value    | Category
----------------------------|----------|----------
Receipt.ReceiptTemplate     | modern   | Receipt
```

---

## Success Indicators

✅ All console logs appear in correct sequence  
✅ API returns 'modern' after save  
✅ UI shows modern template selected after refresh  
✅ Database shows 'modern' value  
✅ No errors in console  
✅ Selection persists across browser restart  

---

## If Issue Still Persists

### Scenario A: Console shows 'modern' but UI shows 'standard'

**Diagnosis:** Rendering issue  
**Fix:** The `key` prop should force re-render. Check if template selection is hardcoded somewhere.

**Additional Debug:**
Add this inside the template selection loop in EnhancedReceiptSettings.tsx:

```typescript
{RECEIPT_TEMPLATES.map(template => (
  <Col md={6} lg={3} key={template.id}>
    <Card 
      className={`h-100 cursor-pointer hover-shadow transition-all ${
        settings.receiptTemplate === template.id ? 'border-primary border-3' : 'border'
      }`}
      onClick={() => {
        console.log(`🖱️ Template clicked: ${template.id}, current: ${settings.receiptTemplate}`);
        setSettings({...settings, receiptTemplate: template.id});
        // ... rest of click handler
      }}
    >
      {/* Add visual debug inside each card */}
      {console.log(`Rendering ${template.id}, selected=${settings.receiptTemplate === template.id}`)}
      {/* ... rest of card */}
    </Card>
  </Col>
))}
```

### Scenario B: API returns 'standard' even after save

**Diagnosis:** Backend not saving  
**Fix:** Check backend logs for errors during save

**Backend Debug:** Add logging to SystemSettingsService.cs UpdateSettingAsync:

```csharp
public async Task UpdateSettingAsync(string key, string value, CancellationToken cancellationToken = default)
{
    Console.WriteLine($"📝 UpdateSetting: {key} = {value}");
    
    var setting = await _context.SystemSettings
        .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

    if (setting == null)
    {
        Console.WriteLine($"➕ Creating new setting: {key}");
        var category = key.Split('.')[0];
        setting = new SystemSetting { Key = key, Value = value, Category = category };
        await _context.SystemSettings.AddAsync(setting, cancellationToken);
    }
    else
    {
        Console.WriteLine($"♻️ Updating existing setting: {key} from '{setting.Value}' to '{value}'");
        setting.Value = value;
        _context.SystemSettings.Update(setting);
    }

    await _context.SaveChangesAsync(cancellationToken);
    Console.WriteLine($"✅ Saved: {key} = {value}");
}
```

### Scenario C: Database shows 'modern' but API returns 'standard'

**Diagnosis:** Caching issue in backend  
**Fix:** Check if Entity Framework is caching entities

**Solution:** Ensure no caching in GetReceiptSettingsAsync:

```csharp
public async Task<ReceiptSettingsDto> GetReceiptSettingsAsync(CancellationToken cancellationToken = default)
{
    // Force fresh read from database
    var templateValue = await _context.SystemSettings
        .AsNoTracking() // Add this to prevent caching
        .Where(s => s.Key == "Receipt.ReceiptTemplate")
        .Select(s => s.Value)
        .FirstOrDefaultAsync(cancellationToken);
    
    Console.WriteLine($"📖 Read Receipt.ReceiptTemplate from DB: {templateValue ?? "null"}");
    
    return new ReceiptSettingsDto
    {
        // ... other fields
        ReceiptTemplate = templateValue ?? "standard",
        // ... other fields
    };
}
```

---

## Emergency Fallback

If all else fails, use localStorage as temporary persistence:

**Add to EnhancedReceiptSettings.tsx:**

```typescript
// Save to localStorage on template change
const handleTemplateChange = (templateId: string) => {
  setSettings({...settings, receiptTemplate: templateId});
  localStorage.setItem('lastSelectedTemplate', templateId);
  console.log(`💾 Saved to localStorage: ${templateId}`);
};

// Load from localStorage on mount
useEffect(() => {
  const cached = localStorage.getItem('lastSelectedTemplate');
  if (cached && cached !== settings.receiptTemplate) {
    console.log(`📦 Found cached template: ${cached}`);
    setSettings(prev => ({...prev, receiptTemplate: cached}));
  }
}, []);
```

**Note:** This is a workaround, not a solution. The database should be the source of truth.

---

## Summary

This implementation adds:

1. ✅ Comprehensive console logging at every step
2. ✅ Cache-busting headers on API requests
3. ✅ Force reload after save to verify persistence
4. ✅ Component key to force re-render on changes
5. ✅ Enhanced useEffect dependency tracking
6. ✅ Debug helpers for troubleshooting

The issue should now be identifiable from console logs, and the fixes should resolve the persistence problem! 🎯
