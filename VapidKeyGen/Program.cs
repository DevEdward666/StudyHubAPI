﻿// Generate VAPID keys using web-push-codelab method
// In this library version, we need to use the ECDsa approach

using System.Security.Cryptography;

var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var privateKey = ecdsa.ExportECPrivateKey();
var publicKey = ecdsa.ExportSubjectPublicKeyInfo();

var publicKeyBase64 = Convert.ToBase64String(publicKey).TrimEnd('=').Replace('+', '-').Replace('/', '_');
var privateKeyBase64 = Convert.ToBase64String(privateKey).TrimEnd('=').Replace('+', '-').Replace('/', '_');

Console.WriteLine("=== VAPID Keys Generated ===");
Console.WriteLine($"Public Key: {publicKeyBase64}");
Console.WriteLine($"Private Key: {privateKeyBase64}");
Console.WriteLine();
Console.WriteLine("Add these to your appsettings.json under WebPush section:");
Console.WriteLine($"  \"VapidPublicKey\": \"{publicKeyBase64}\",");
Console.WriteLine($"  \"VapidPrivateKey\": \"{privateKeyBase64}\",");


