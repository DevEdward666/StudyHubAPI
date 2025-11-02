# Thermal Receipt Printer Implementation

## Overview
Professional thermal receipt printing system for the Study Hub cashier application, designed to work with the RPP02N-1175 Bluetooth thermal printer.

## Features
- ✅ Automatic receipt printing when a table session starts
- ✅ Professional Starbucks-style receipt layout
- ✅ QR code for WiFi password (scannable by phone)
- ✅ ESC/POS command support for thermal printers
- ✅ Manual reprint functionality
- ✅ Receipt preview/download endpoint

## Architecture

### Components Created

1. **IThermalPrinterService** (`Service/Interface/IThermalPrinterService.cs`)
   - Interface for thermal printer operations
   
2. **ThermalPrinterService** (`Service/ThermalPrinterService.cs`)
   - Implements ESC/POS commands for 58mm thermal printers
   - Generates QR codes using QRCoder library
   - Formats receipts professionally

3. **ReceiptDto** (`Models/DTOs/ReceiptDto.cs`)
   - Data transfer object for receipt information

4. **TablesController Updates**
   - Automatic printing on session start
   - Manual print endpoint: `POST /api/tables/sessions/{sessionId}/print-receipt`
   - Preview endpoint: `GET /api/tables/sessions/{sessionId}/receipt-preview`

## Receipt Layout

```
================================
       STUDY HUB
    Your Business Address
   Contact: 09XX-XXX-XXXX
================================

TRANSACTION RECEIPT

Trans ID:   abc12345
Date:       Nov 02, 2025
Time:       02:30 PM

Customer:   John Doe
--------------------------------

SESSION DETAILS
Table:      Table 1
Start:      02:30 PM
End:        04:30 PM
Duration:   2.00 hours

--------------------------------

PAYMENT
Rate/Hour:  ₱50.00
Hours:      2.00
--------------------------------

TOTAL:      ₱100.00

Method:     Cash
Cash:       ₱150.00
Change:     ₱50.00

================================

     FREE WIFI ACCESS

      [QR CODE HERE]
      
      Scan QR Code
   Password: password1234

================================
Thank you for studying with us!
    Have a productive day!


```

## API Endpoints

### 1. Start Session (Auto-Print)
```http
POST /api/tables/sessions/start
Authorization: Bearer {token}
Content-Type: application/json

{
  "tableId": "guid",
  "hours": 2,
  "amount": 100.00,
  "paymentMethod": "Cash",
  "cash": 150.00,
  "change": 50.00
}
```

**Response:**
```json
{
  "success": true,
  "data": "session-guid",
  "message": "Session started successfully"
}
```
*Receipt will automatically print in the background*

### 2. Print/Reprint Receipt
```http
POST /api/tables/sessions/{sessionId}/print-receipt
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": true,
  "message": "Receipt printed successfully"
}
```

### 3. Preview/Download Receipt
```http
GET /api/tables/sessions/{sessionId}/receipt-preview
Authorization: Bearer {token}
```

**Response:** Binary file (ESC/POS commands)

## ESC/POS Commands Used

### Text Formatting
- `ESC @` - Initialize printer
- `ESC a n` - Text alignment (0=left, 1=center, 2=right)
- `ESC E n` - Bold (1=on, 0=off)
- `ESC ! n` - Character size

### QR Code
- `GS ( k` - QR code generation commands
- Model 2 QR code with error correction level M
- Size 8 (adjustable based on printer)

### Paper Control
- `ESC d n` - Feed n lines
- `GS V` - Cut paper

## Configuration

### Business Information
Update in `TablesController.cs` or move to configuration:

```csharp
var receipt = new ReceiptDto
{
    BusinessName = "Study Hub",
    BusinessAddress = "123 Main St, City",
    BusinessContact = "Contact: 0917-123-4567",
    WifiPassword = "password1234",
    // ... other fields
};
```

### Printer Connection (TODO)

The current implementation saves to a temporary file. To connect to your RPP02N-1175 printer, implement Bluetooth communication:

```csharp
// In ThermalPrinterService.PrintReceiptAsync
// Example pseudo-code:
using (var bluetoothClient = new BluetoothClient())
{
    var device = bluetoothClient.DiscoverDevices()
        .FirstOrDefault(d => d.DeviceName == "RPP02N-1175");
    
    if (device != null)
    {
        var stream = bluetoothClient.GetStream();
        await stream.WriteAsync(receiptData, 0, receiptData.Length);
    }
}
```

**Recommended Libraries:**
- `InTheHand.Net.Bluetooth` for Bluetooth connectivity
- `32feet.NET` for Windows Bluetooth support

## Testing

### 1. Test with Postman or cURL

**Start a session:**
```bash
curl -X POST "http://localhost:5000/api/tables/sessions/start" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "tableId": "your-table-guid",
    "hours": 2,
    "amount": 100.00,
    "paymentMethod": "Cash",
    "cash": 150.00,
    "change": 50.00
  }'
```

**Reprint a receipt:**
```bash
curl -X POST "http://localhost:5000/api/tables/sessions/{sessionId}/print-receipt" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Download receipt preview:**
```bash
curl -X GET "http://localhost:5000/api/tables/sessions/{sessionId}/receipt-preview" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output receipt.bin
```

### 2. Verify Receipt File

The receipt is saved to your system's temp directory:
```bash
# macOS/Linux
ls /tmp/receipt_*.bin

# Windows
dir %TEMP%\receipt_*.bin
```

### 3. Test with Actual Printer

To test with your RPP02N-1175:
1. Pair the printer via Bluetooth
2. Implement Bluetooth communication in `ThermalPrinterService.PrintReceiptAsync`
3. Send the byte array to the printer

## Customization

### Change Receipt Width
Default is 32 characters for 58mm printers. Adjust in `ThermalPrinterService.cs`:

```csharp
private byte[] PrintRow(string label, string value, int totalWidth = 32)
```

For 80mm printers, use `totalWidth = 48`

### Modify QR Code Size
```csharp
// In GenerateQRCodeAsync
var qrCodeImage = qrCode.GetGraphic(8); // Change size here (1-10)
```

### Add Logo/Image
Add image printing commands before the business name:
```csharp
// ESC/POS image printing (requires bitmap conversion)
commands.AddRange(PrintLogo(logoBytes));
```

## Troubleshooting

### Receipt Not Printing
1. Check Bluetooth connection
2. Verify printer is powered on and has paper
3. Check temp directory for generated file
4. Review application logs for errors

### QR Code Not Scanning
1. Increase QR code size
2. Ensure adequate spacing around QR code
3. Check printer DPI settings
4. Verify WiFi password is correct

### Text Misaligned
1. Adjust `totalWidth` parameter
2. Check character encoding (UTF-8)
3. Verify printer paper width

## Dependencies

- **QRCoder** (v1.7.0) - QR code generation
- **System.Drawing.Common** (v6.0.0) - Image processing

## Future Enhancements

- [ ] Add Bluetooth printer auto-discovery
- [ ] Support multiple printer models
- [ ] Add printer status monitoring
- [ ] Implement print queue for reliability
- [ ] Add receipt templates/themes
- [ ] Support custom logos
- [ ] Email receipt option
- [ ] SMS receipt option
- [ ] Receipt history/archive

## Notes

- Receipt prints asynchronously to avoid blocking session creation
- QR code contains plain text password (consider encryption for production)
- Temp files are automatically cleaned by the OS
- ESC/POS commands are printer-model-specific (tested for standard 58mm thermal printers)

## Support

For issues or questions:
1. Check printer manufacturer documentation
2. Verify ESC/POS command compatibility
3. Test with receipt preview endpoint first
4. Review application logs for detailed errors

