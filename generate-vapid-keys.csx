using Lib.Net.Http.WebPush.Authentication;

// Generate VAPID keys
var keys = VapidHelper.GenerateVapidKeys();

Console.WriteLine("=== VAPID Keys Generated ===");
Console.WriteLine($"Public Key: {keys.PublicKey}");
Console.WriteLine($"Private Key: {keys.PrivateKey}");
Console.WriteLine();
Console.WriteLine("Add these to your appsettings.json under WebPush section:");
Console.WriteLine($"  \"VapidPublicKey\": \"{keys.PublicKey}\",");
Console.WriteLine($"  \"VapidPrivateKey\": \"{keys.PrivateKey}\",");

