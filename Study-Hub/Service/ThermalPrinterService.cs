using Study_Hub.Models.DTOs;
using Study_Hub.Service.Interface;
using System.Text;
using System.IO.Ports;

namespace Study_Hub.Service
{
    public class ThermalPrinterService : IThermalPrinterService
    {
        // ESC/POS Commands
        private static readonly byte[] ESC = { 0x1B };
        private static readonly byte[] GS = { 0x1D };
        private static readonly byte[] LF = { 0x0A };
        private static readonly byte[] CR = { 0x0D };
        
        // Initialize printer
        private static readonly byte[] INIT = { 0x1B, 0x40 };
        
        // Text alignment
        private static readonly byte[] ALIGN_LEFT = { 0x1B, 0x61, 0x00 };
        private static readonly byte[] ALIGN_CENTER = { 0x1B, 0x61, 0x01 };
        private static readonly byte[] ALIGN_RIGHT = { 0x1B, 0x61, 0x02 };
        
        // Text styles
        private static readonly byte[] BOLD_ON = { 0x1B, 0x45, 0x01 };
        private static readonly byte[] BOLD_OFF = { 0x1B, 0x45, 0x00 };
        private static readonly byte[] DOUBLE_HEIGHT_ON = { 0x1B, 0x21, 0x10 };
        private static readonly byte[] DOUBLE_WIDTH_ON = { 0x1B, 0x21, 0x20 };
        private static readonly byte[] DOUBLE_SIZE_ON = { 0x1B, 0x21, 0x30 };
        private static readonly byte[] NORMAL_SIZE = { 0x1B, 0x21, 0x00 };
        private static readonly byte[] MEDIUM_SIZE = { 0x1B, 0x21, 0x10 }; // roughly 18px high

        
        // Cut paper
        private static readonly byte[] CUT_PAPER = { 0x1D, 0x56, 0x41, 0x00 };
        
        // Feed lines
        private static readonly byte[] FEED_3_LINES = { 0x1B, 0x64, 0x03 };

        public async Task<byte[]> GenerateReceiptAsync(ReceiptDto receipt)
        {
            var commands = new List<byte>();
            
            // Initialize printer
            commands.AddRange(INIT);
            
            // Header - Business Name (compact)
            commands.AddRange(ALIGN_CENTER);
            commands.AddRange(DOUBLE_WIDTH_ON);
            commands.AddRange(BOLD_ON);
            commands.AddRange(Encoding.UTF8.GetBytes(receipt.BusinessName));
            commands.AddRange(NORMAL_SIZE);
            commands.AddRange(BOLD_OFF);
            commands.AddRange(LF);
            
            // Business Info (compact - only if not empty)
            if (!string.IsNullOrEmpty(receipt.BusinessContact))
            {
                commands.AddRange(Encoding.UTF8.GetBytes(receipt.BusinessContact));
                commands.AddRange(LF);
            }
            commands.AddRange(PrintLine("=", 32));
            
            // Transaction Details (compact)
            commands.AddRange(ALIGN_LEFT);
            commands.AddRange(PrintRow("ID:", receipt.TransactionId.Substring(0, Math.Min(8, receipt.TransactionId.Length))));
            commands.AddRange(PrintRow("Date:", receipt.TransactionDate.ToString("MMM dd, yyyy hh:mm tt")));
            commands.AddRange(PrintRow("Customer:", receipt.CustomerName));
            commands.AddRange(PrintRow("Table:", receipt.TableNumber));
            commands.AddRange(PrintLine("-", 32));
            
            // Session Details (compact)
            commands.AddRange(PrintRow("Start:", receipt.StartTime.ToString("hh:mm tt")));
            commands.AddRange(PrintRow("End:", receipt.EndTime.ToString("hh:mm tt")));
            commands.AddRange(PrintRow("Duration:", $"{receipt.Hours:F2} hrs"));
            commands.AddRange(PrintLine("-", 32));
            
            // Payment Details (compact with ₱) - calculate rate dynamically
            var calculatedRate = receipt.Hours > 0 ? receipt.TotalAmount / (decimal)receipt.Hours : 0;
            commands.AddRange(PrintRow("Rate/Hour:", $"Php: {calculatedRate:F2}"));
            commands.AddRange(PrintRow("Hours:", $"{receipt.Hours:F2}"));
            commands.AddRange(PrintLine("-", 32));
            
            // Total Amount (bold, large)
            commands.AddRange(BOLD_ON);
            commands.AddRange(NORMAL_SIZE);
            commands.AddRange(PrintRow("TOTAL:", $"Php: {receipt.TotalAmount:F2}"));
            commands.AddRange(NORMAL_SIZE);
            commands.AddRange(BOLD_OFF);
            
            // Payment Method and Cash/Change (with Php: )
            commands.AddRange(PrintRow("Method:", receipt.PaymentMethod));
            if (receipt.Cash.HasValue && receipt.Cash.Value > 0)
            {
                commands.AddRange(PrintRow("Cash:", $"Php: {receipt.Cash.Value:F2}"));
            }
            if (receipt.Change.HasValue && receipt.Change.Value > 0)
            {
                commands.AddRange(PrintRow("Change:", $"Php: {receipt.Change.Value:F2}"));
            }
            commands.AddRange(PrintLine("=", 32));

            // WiFi Access (compact - WITH QR CODE)
            commands.AddRange(ALIGN_CENTER);
            commands.AddRange(BOLD_ON);
            commands.AddRange(Encoding.UTF8.GetBytes("FREE WIFI"));
            commands.AddRange(BOLD_OFF);

            commands.AddRange(LF);

            // QR Code for WiFi Password
            var qrCode = await GenerateQRCodeAsync(receipt.WifiPassword);
            if (qrCode.Length > 0)
            {
                
                commands.AddRange(qrCode);
                commands.AddRange(LF);
                commands.AddRange(Encoding.UTF8.GetBytes("Scan QR Code"));
                commands.AddRange(LF);
            }

            // Password text (compact)
            commands.AddRange(BOLD_ON);
            commands.AddRange(Encoding.UTF8.GetBytes($"WiFi: {receipt.WifiPassword}"));
            commands.AddRange(BOLD_OFF);
            commands.AddRange(LF);
            commands.AddRange(PrintLine("=", 32));
            
            // Footer (compact)
            commands.AddRange(Encoding.UTF8.GetBytes("Thank you!"));
            commands.AddRange(LF);
            commands.AddRange(LF);
            
            // Feed and cut
            commands.AddRange(CUT_PAPER);
            
            return commands.ToArray();
        }

        public async Task<bool> PrintReceiptAsync(ReceiptDto receipt, bool waitForCompletion = true, int timeoutMs = 15000)
        {
            try
            {
                var receiptData = await GenerateReceiptAsync(receipt);
                
                if (waitForCompletion)
                {
                    // Server/USB mode: Wait for actual print completion
                    Console.WriteLine("🖨️ Print job started (waiting for completion)...");
                    Console.WriteLine($"📊 Receipt data: {receiptData.Length} bytes");
                    Console.WriteLine($"⏱️  Timeout: {timeoutMs}ms");
                    
                    var printTask = TryPrintAsync(receiptData);
                    var timeoutTask = Task.Delay(timeoutMs);
                    var completedTask = await Task.WhenAny(printTask, timeoutTask);

                    if (completedTask == printTask)
                    {
                        var printSuccess = await printTask;
                        if (printSuccess)
                        {
                            Console.WriteLine("✅ Receipt printed successfully");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("❌ Print attempt failed");
                            Console.WriteLine("⚠️ Saving receipt to file as fallback...");
                            try
                            {
                                var filePath = Path.Combine(Path.GetTempPath(), $"receipt_{DateTime.Now:yyyyMMddHHmmss}.bin");
                                await File.WriteAllBytesAsync(filePath, receiptData);
                                Console.WriteLine($"📄 Receipt saved to: {filePath}");
                                Console.WriteLine("💡 Check printer connection and permissions");
                            }
                            catch (Exception saveEx)
                            {
                                Console.WriteLine($"❌ Failed to save receipt fallback: {saveEx.Message}");
                            }
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⏳ Print timed out after {timeoutMs}ms");
                        Console.WriteLine("⚠️ Saving receipt to file as fallback...");
                        try
                        {
                            var filePath = Path.Combine(Path.GetTempPath(), $"receipt_{DateTime.Now:yyyyMMddHHmmss}.bin");
                            await File.WriteAllBytesAsync(filePath, receiptData);
                            Console.WriteLine($"📄 Receipt saved to: {filePath}");
                        }
                        catch (Exception saveEx)
                        {
                            Console.WriteLine($"❌ Failed to save receipt after timeout: {saveEx.Message}");
                        }
                        return false;
                    }
                }
                else
                {
                    // Non-blocking mode: Fire-and-forget (for Bluetooth/development)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            Console.WriteLine("Starting background print job...");
                            var printSuccess = await TryPrintAsync(receiptData);
                            
                            if (printSuccess)
                            {
                                Console.WriteLine("✅ Receipt printed successfully (background)");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Background printing failed. Saving to file...");
                                var filePath = Path.Combine(Path.GetTempPath(), $"receipt_{DateTime.Now:yyyyMMddHHmmss}.bin");
                                await File.WriteAllBytesAsync(filePath, receiptData);
                                Console.WriteLine($"📄 Receipt saved to: {filePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Background print error: {ex.Message}");
                            try
                            {
                                var filePath = Path.Combine(Path.GetTempPath(), $"receipt_{DateTime.Now:yyyyMMddHHmmss}.bin");
                                await File.WriteAllBytesAsync(filePath, receiptData);
                                Console.WriteLine($"📄 Receipt saved to file after error: {filePath}");
                            }
                            catch (Exception saveEx)
                            {
                                Console.WriteLine($"❌ Failed to save receipt: {saveEx.Message}");
                            }
                        }
                    });
                    
                    Console.WriteLine("🖨️ Print job queued successfully (background)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PrintReceiptAsync: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task<bool> TryPrintAsync(byte[] receiptData)
        {
            try
            {
                Console.WriteLine("🔍 Starting printer detection...");
                Console.WriteLine($"📊 Data size: {receiptData.Length} bytes");
                Console.WriteLine($"💻 OS: {Environment.OSVersion.Platform}");
                Console.WriteLine($"👤 User: {Environment.UserName}");
                Console.WriteLine($"📁 Temp path: {Path.GetTempPath()}");
                
                // Priority 1: Try CUPS printer (macOS/Linux Printers & Scanners)
                var cupsPrinterName = FindCupsPrinter();
                if (!string.IsNullOrEmpty(cupsPrinterName))
                {
                    Console.WriteLine($"✅ Found CUPS printer: {cupsPrinterName}");
                    Console.WriteLine($"🖨️  Attempting CUPS print...");
                    var success = await PrintViaCups(cupsPrinterName, receiptData);
                    if (success)
                    {
                        Console.WriteLine($"✅ CUPS print completed successfully");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ CUPS print failed, trying serial port...");
                    }
                }
                else
                {
                    Console.WriteLine($"ℹ️  No CUPS printer found, checking serial ports...");
                }
                
                // Priority 2: Try to find printer via serial port (USB or Bluetooth)
                var printerPort = FindPrinterPort();
                
                if (!string.IsNullOrEmpty(printerPort))
                {
                    Console.WriteLine($"✅ Found serial port: {printerPort}");
                    Console.WriteLine($"🖨️  Attempting serial port print...");
                    var success = await SendToSerialPortAsync(printerPort, receiptData);
                    if (success)
                    {
                        Console.WriteLine($"✅ Serial port print completed successfully");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Serial port print failed");
                    }
                }
                else
                {
                    Console.WriteLine($"ℹ️  No serial port found");
                }
                
                // Priority 3: If serial port not found on Windows, try direct Bluetooth
                #if WINDOWS
                Console.WriteLine($"🔵 Attempting Windows Bluetooth...");
                return await SendViaBluetoothAsync(receiptData);
                #else
                Console.WriteLine("❌ No printer found. Troubleshooting steps:");
                Console.WriteLine("1. CUPS: Run 'lpstat -p' to see available printers");
                Console.WriteLine("2. USB: Run 'ls -la /dev/cu.* | grep -i usb' to see USB devices");
                Console.WriteLine("3. USB: Check permissions with 'ls -la /dev/cu.usbserial*'");
                Console.WriteLine("4. Server: Ensure printer is connected and powered on");
                Console.WriteLine("5. Server: Check if user has permissions to access /dev ports");
                return false;
                #endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Printer connection error: {ex.GetType().Name}");
                Console.WriteLine($"❌ Message: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private string? FindCupsPrinter()
        {
            try
            {
                Console.WriteLine("🔍 Searching for CUPS printers (Printers & Scanners)...");
                
                // Run lpstat to get list of printers
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/lpstat",
                        Arguments = "-p",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine("📋 Available CUPS printers:");
                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var line in lines)
                    {
                        Console.WriteLine($"   {line}");
                        
                        // Extract printer name from: "printer Manufacture_Virtual_PRN is idle..."
                        if (line.StartsWith("printer "))
                        {
                            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2)
                            {
                                var printerName = parts[1];
                                Console.WriteLine($"✅ Found CUPS printer: {printerName}");
                                Console.WriteLine($"🖨️  Connection type: CUPS (Printers & Scanners)");
                                return printerName;
                            }
                        }
                    }
                }
                
                Console.WriteLine("ℹ️  No CUPS printers found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not query CUPS printers: {ex.Message}");
                return null;
            }
        }

        private async Task<bool> PrintViaCups(string printerName, byte[] receiptData)
        {
            try
            {
                Console.WriteLine($"🖨️  Printing via CUPS printer: {printerName}");
                Console.WriteLine($"📊 Data size: {receiptData.Length} bytes");
                
                // Create temporary file with receipt data
                var tempFile = Path.Combine(Path.GetTempPath(), $"receipt_{DateTime.Now:yyyyMMddHHmmss}.bin");
                await File.WriteAllBytesAsync(tempFile, receiptData);
                
                Console.WriteLine($"📄 Saved receipt to: {tempFile}");
                
                // Print using lp command with raw mode
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/lp",
                        Arguments = $"-d {printerName} -o raw {tempFile}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                Console.WriteLine($"🔄 Executing: lp -d {printerName} -o raw {tempFile}");
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"✅ Print job submitted successfully");
                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"   {output.Trim()}");
                    }
                    
                    // Clean up temp file after a delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                                Console.WriteLine($"🧹 Cleaned up temp file: {tempFile}");
                            }
                        }
                        catch { }
                    });
                    
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Print job failed with exit code: {process.ExitCode}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"   Error: {error.Trim()}");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CUPS printing error: {ex.Message}");
                return false;
            }
        }

        private string? FindPrinterPort()
        {
            try
            {
                Console.WriteLine("🔍 Searching for printer ports...");
                
                if (Directory.Exists("/dev/"))
                {
                    // macOS/Linux: List all available serial ports
                    Console.WriteLine("📋 Scanning /dev/ for serial ports...");
                    
                    var allPorts = Directory.GetFiles("/dev/")
                        .Where(f => f.Contains("cu.") || f.Contains("tty."))
                        .OrderBy(f => f)
                        .ToList();
                    
                    Console.WriteLine($"📊 Found {allPorts.Count} potential serial ports");
                    
                    if (allPorts.Any())
                    {
                        Console.WriteLine($"📋 Available serial ports:");
                        foreach (var port in allPorts)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(port);
                                var permissions = fileInfo.UnixFileMode;
                                Console.WriteLine($"   - {port} (permissions: {permissions})");
                            }
                            catch
                            {
                                Console.WriteLine($"   - {port} (permissions: unknown)");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No serial ports found in /dev/");
                    }
                    
                    // Priority 1: Look for USB serial devices (common patterns)
                    var usbPatterns = new[] { "usbserial", "usbmodem", "USB", "usb" };
                    foreach (var pattern in usbPatterns)
                    {
                        var usbPort = allPorts.FirstOrDefault(f => 
                            f.Contains(pattern, StringComparison.OrdinalIgnoreCase) &&
                            f.Contains("cu.")
                        );
                        
                        if (usbPort != null)
                        {
                            Console.WriteLine($"✅ Found USB printer port: {usbPort}");
                            Console.WriteLine($"🔌 Connection type: USB");
                            Console.WriteLine($"📝 Pattern matched: {pattern}");
                            
                            // Check if port is accessible
                            try
                            {
                                var fileInfo = new FileInfo(usbPort);
                                if (fileInfo.Exists)
                                {
                                    Console.WriteLine($"✅ Port exists and is accessible");
                                    return usbPort;
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Port exists but may not be accessible");
                                    return usbPort;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Port access check failed: {ex.Message}");
                                return usbPort; // Try anyway
                            }
                        }
                    }
                    
                    // Priority 2: Look for RPP02N Bluetooth specifically
                    var rpp02nPort = allPorts.FirstOrDefault(f => 
                        f.Contains("RPP02N", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("Incoming", StringComparison.OrdinalIgnoreCase)
                    );
                    
                    if (rpp02nPort != null)
                    {
                        Console.WriteLine($"✅ Found RPP02N Bluetooth port: {rpp02nPort}");
                        Console.WriteLine($"📡 Connection type: Bluetooth");
                        return rpp02nPort;
                    }
                    
                    // Priority 3: Look for any device with "SerialPort" in name (Bluetooth)
                    var serialPort = allPorts.FirstOrDefault(f => 
                        f.Contains("SerialPort", StringComparison.OrdinalIgnoreCase) &&
                        f.Contains("cu.") &&
                        !f.Contains("Incoming", StringComparison.OrdinalIgnoreCase)
                    );
                    
                    if (serialPort != null)
                    {
                        Console.WriteLine($"✅ Found Bluetooth SerialPort: {serialPort}");
                        Console.WriteLine($"📡 Connection type: Bluetooth");
                        return serialPort;
                    }
                    
                    // Priority 4: Look for any Bluetooth device (excluding Incoming-Port)
                    var bluetoothPort = allPorts.FirstOrDefault(f => 
                        f.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("Incoming-Port", StringComparison.OrdinalIgnoreCase) &&
                        f.Contains("cu.")
                    );
                    
                    if (bluetoothPort != null)
                    {
                        Console.WriteLine($"✅ Found Bluetooth port: {bluetoothPort}");
                        Console.WriteLine($"📡 Connection type: Bluetooth");
                        return bluetoothPort;
                    }
                    
                    // Check if we only found Incoming-Port
                    var incomingPort = allPorts.FirstOrDefault(f => 
                        f.Contains("Bluetooth-Incoming-Port", StringComparison.OrdinalIgnoreCase)
                    );
                    
                    if (incomingPort != null)
                    {
                        Console.WriteLine("⚠️ Found only Bluetooth-Incoming-Port (wrong direction)");
                        Console.WriteLine("⚠️ Try connecting via USB or re-pair Bluetooth printer");
                    }
                }
                else
                {
                    // Windows: Check COM ports (both USB and Bluetooth)
                    Console.WriteLine("📋 Scanning Windows COM ports...");
                    var ports = SerialPort.GetPortNames();
                    if (ports.Any())
                    {
                        Console.WriteLine($"📋 Available COM ports: {string.Join(", ", (IEnumerable<string>)ports)}");
                        Console.WriteLine($"💡 These may be USB or Bluetooth connections");
                        
                        // Return first available port
                        var selectedPort = ports.FirstOrDefault();
                        if (selectedPort != null)
                        {
                            Console.WriteLine($"✅ Selected COM port: {selectedPort}");
                            return selectedPort;
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No COM ports found");
                    }
                }

                Console.WriteLine("❌ No printer port found");
                Console.WriteLine("💡 Troubleshooting:");
                Console.WriteLine("   1. Check if printer is connected and powered on");
                Console.WriteLine("   2. Run 'ls -la /dev/cu.* | grep -i usb' to see USB devices");
                Console.WriteLine("   3. Run 'ls -la /dev/tty.* | grep -i usb' to see tty devices");
                Console.WriteLine("   4. Check permissions: current user may need access to /dev ports");
                Console.WriteLine("   5. Try: sudo chmod 666 /dev/cu.usbserial* (if USB device found)");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error finding printer port: {ex.GetType().Name}");
                Console.WriteLine($"❌ Message: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private async Task<bool> SendToSerialPortAsync(string portName, byte[] data)
        {
            const int maxRetries = 3;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                Console.WriteLine($"🔄 Print attempt {attempt}/{maxRetries}...");
                
                SerialPort? serialPort = null;
                
                try
                {
                    Console.WriteLine($"📡 Connecting to printer on {portName}...");
                    
                    serialPort = new SerialPort(portName)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        ReadTimeout = 5000,  // Increased to 5 seconds
                        WriteTimeout = 5000, // Increased to 5 seconds
                        DtrEnable = true,
                        RtsEnable = true
                    };

                    serialPort.Open();
                    
                    // Wait for port to stabilize
                    await Task.Delay(500);
                    
                    if (!serialPort.IsOpen)
                    {
                        throw new IOException("Failed to open serial port");
                    }
                    
                    Console.WriteLine($"✅ Port opened successfully, sending {data.Length} bytes...");
                    
                    // Detect connection type and adjust speed accordingly
                    bool isUSB = portName.Contains("usbserial", StringComparison.OrdinalIgnoreCase) ||
                                 portName.Contains("usbmodem", StringComparison.OrdinalIgnoreCase) ||
                                 portName.Contains("USB", StringComparison.OrdinalIgnoreCase);
                    
                    int chunkSize;
                    int delayMs;
                    int postPrintDelay;
                    
                    if (isUSB)
                    {
                        // USB is more reliable - use faster settings
                        chunkSize = 512;
                        delayMs = 50;
                        postPrintDelay = 1000;
                        Console.WriteLine($"🔌 USB connection detected - using FAST mode");
                    }
                    else
                    {
                        // Bluetooth - use ultra-slow settings for RSSI -57
                        chunkSize = 64;
                        delayMs = 500;
                        postPrintDelay = 4000;
                        Console.WriteLine($"📡 Bluetooth connection detected - using ULTRA-SLOW mode");
                    }
                    int totalSent = 0;
                    int chunkNumber = 0;
                    
                    for (int i = 0; i < data.Length; i += chunkSize)
                    {
                        chunkNumber++;
                        int currentChunkSize = Math.Min(chunkSize, data.Length - i);
                        
                        Console.WriteLine($"📤 Sending chunk {chunkNumber}/{(int)Math.Ceiling((double)data.Length / chunkSize)} ({currentChunkSize} bytes)...");
                        
                        // Check port is still open
                        if (!serialPort.IsOpen)
                        {
                            throw new IOException($"Port closed at chunk {chunkNumber}");
                        }
                        
                        await serialPort.BaseStream.WriteAsync(data, i, currentChunkSize);
                        await serialPort.BaseStream.FlushAsync();
                        totalSent += currentChunkSize;
                        
                        Console.WriteLine($"✓ Progress: {totalSent}/{data.Length} bytes ({(totalSent * 100 / data.Length)}%)");
                        
                        // Variable delay based on connection type
                        if (i + chunkSize < data.Length)
                        {
                            await Task.Delay(delayMs);
                        }
                    }
                    
                    // Give printer time based on connection type
                    Console.WriteLine($"⏳ Waiting {postPrintDelay}ms for printer to complete...");
                    await Task.Delay(postPrintDelay);
                    
                    // Close port
                    Console.WriteLine("🔓 Closing port...");
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        serialPort.Dispose();
                    }
                    
                    Console.WriteLine($"✅ Print completed successfully on attempt {attempt}!");
                    Console.WriteLine($"✅ Sent {totalSent} bytes in {chunkNumber} chunks");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Attempt {attempt} failed: {ex.GetType().Name} - {ex.Message}");
                    
                    // Cleanup
                    try
                    {
                        if (serialPort != null && serialPort.IsOpen)
                        {
                            serialPort.Close();
                            serialPort.Dispose();
                        }
                    }
                    catch { }
                    
                    // Retry logic
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"⏳ Waiting 3 seconds before retry {attempt + 1}...");
                        await Task.Delay(3000);
                    }
                    else
                    {
                        Console.WriteLine($"❌ All {maxRetries} attempts failed. Giving up.");
                        return false;
                    }
                }
            }
            
            return false;
        }

        #if WINDOWS
        private async Task<bool> SendViaBluetoothAsync(byte[] data)
        {
            try
            {
                // Windows Bluetooth implementation using InTheHand.Net.Bluetooth
                // Note: This requires InTheHand.Net.Bluetooth package
                
                using var client = new InTheHand.Net.Sockets.BluetoothClient();
                
                // Discover devices
                var devices = client.DiscoverDevices();
                var printer = devices.FirstOrDefault(d => 
                    d.DeviceName.Contains("RPP02N", StringComparison.OrdinalIgnoreCase) ||
                    d.DeviceName.Contains("1175", StringComparison.OrdinalIgnoreCase)
                );

                if (printer == null)
                {
                    Console.WriteLine("RPP02N-1175 printer not found in paired devices.");
                    Console.WriteLine("Please pair the printer via Windows Bluetooth settings.");
                    return false;
                }

                Console.WriteLine($"Found printer: {printer.DeviceName}");

                // Connect to Serial Port Profile (SPP)
                var serviceClass = InTheHand.Net.Bluetooth.BluetoothService.SerialPort;
                client.Connect(printer.DeviceAddress, serviceClass);

                if (client.Connected)
                {
                    var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                    
                    stream.Close();
                    Console.WriteLine($"Successfully sent {data.Length} bytes to Bluetooth printer");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Windows Bluetooth error: {ex.Message}");
                return false;
            }
        }
        #endif

        private byte[] PrintLine(string character, int length)
        {
            var line = new string(character[0], length);
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.UTF8.GetBytes(line));
            bytes.AddRange(LF);
            return bytes.ToArray();
        }

        private byte[] PrintRow(string label, string value, int totalWidth = 32)
        {
            var paddingLength = totalWidth - label.Length - value.Length;
            var padding = paddingLength > 0 ? new string(' ', paddingLength) : " ";
            var row = $"{label}{padding}{value}";
            
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.UTF8.GetBytes(row));
            bytes.AddRange(LF);
            return bytes.ToArray();
        }

        private async Task<byte[]> GenerateQRCodeAsync(string text)
        {
            return await Task.Run(() =>
            {
                var commands = new List<byte>();
                
                try
                {
                    // Use ESC/POS QR Code command (GS ( k)
                    // This is more reliable than bitmap conversion
                    
                    var qrData = Encoding.UTF8.GetBytes(text);
                    var dataLength = qrData.Length + 3;
                    
                    // Store the data in the symbol storage area
                    // Function 165 (0xA5) - QR Code: Store the data in the symbol storage area
                    commands.Add(0x1D); // GS
                    commands.Add(0x28); // (
                    commands.Add(0x6B); // k
                    commands.Add((byte)(dataLength & 0xFF)); // pL
                    commands.Add((byte)(dataLength >> 8));   // pH
                    commands.Add(0x31); // cn (QR Code)
                    commands.Add(0x50); // fn (Store data)
                    commands.Add(0x30); // m (Store to symbol storage)
                    commands.AddRange(qrData);
                    
                    // Set QR Code model
                    // Function 165 (0xA5) - QR Code: Select the model
                    commands.Add(0x1D); // GS
                    commands.Add(0x28); // (
                    commands.Add(0x6B); // k
                    commands.Add(0x04); // pL
                    commands.Add(0x00); // pH
                    commands.Add(0x31); // cn (QR Code)
                    commands.Add(0x41); // fn (Set model)
                    commands.Add(0x32); // n1 (Model 2)
                    commands.Add(0x00); // n2
                    
                    // Set QR Code size
                    // Function 167 (0xA7) - QR Code: Set the size of module
                    commands.Add(0x1D); // GS
                    commands.Add(0x28); // (
                    commands.Add(0x6B); // k
                    commands.Add(0x03); // pL
                    commands.Add(0x00); // pH
                    commands.Add(0x31); // cn (QR Code)
                    commands.Add(0x43); // fn (Set size)
                    commands.Add(0x06); // n (Module size: 6)
                    
                    // Set QR Code error correction level
                    // Function 169 (0xA9) - QR Code: Select the error correction level
                    commands.Add(0x1D); // GS
                    commands.Add(0x28); // (
                    commands.Add(0x6B); // k
                    commands.Add(0x03); // pL
                    commands.Add(0x00); // pH
                    commands.Add(0x31); // cn (QR Code)
                    commands.Add(0x45); // fn (Set error correction)
                    commands.Add(0x31); // n (Level M: 15%)
                    
                    // Print the QR Code
                    // Function 180 (0xB4) - QR Code: Print the symbol data
                    commands.Add(0x1D); // GS
                    commands.Add(0x28); // (
                    commands.Add(0x6B); // k
                    commands.Add(0x03); // pL
                    commands.Add(0x00); // pH
                    commands.Add(0x31); // cn (QR Code)
                    commands.Add(0x51); // fn (Print)
                    commands.Add(0x30); // m (Print stored data)
                    
                    Console.WriteLine($"✅ QR code commands generated for text: {text}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error generating QR code: {ex.Message}");
                    // Return empty array if QR generation fails
                    return new byte[0];
                }
                
                return commands.ToArray();
            });
        }

        private byte[] ConvertImageToEscPos(byte[] imageBytes, int width)
        {
            // This method is no longer used but kept for backward compatibility
            return new byte[0];
        }
    }
}

