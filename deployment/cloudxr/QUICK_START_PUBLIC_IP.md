# Quick Start: Public IP CloudXR Streaming

## 🚀 Fast Setup (5 minutes)

### **1. Get Your Public IP**
```powershell
# Get your public IP address
$publicIP = (Invoke-RestMethod -Uri "https://api.ipify.org")
Write-Host "Your Public IP: $publicIP"
```

### **2. Configure CX Language**
```powershell
# Navigate to CX Language directory
cd C:\Users\YourUsername\cx

# Copy and edit public IP configuration
cp appsettings.PublicIP.json appsettings.Production.json

# Replace YOUR_PUBLIC_IP with your actual public IP
(Get-Content appsettings.Production.json) -replace 'YOUR_PUBLIC_IP', $publicIP | Set-Content appsettings.Production.json
```

### **3. Open Firewall Ports**
```powershell
# Open required ports for CloudXR streaming
New-NetFirewallRule -DisplayName "CloudXR WebSocket" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
New-NetFirewallRule -DisplayName "Unreal Engine CloudXR" -Direction Inbound -Protocol TCP -LocalPort 7777 -Action Allow
New-NetFirewallRule -DisplayName "CloudXR Streaming" -Direction Inbound -Protocol TCP -LocalPort 48010-48200 -Action Allow

Write-Host "✅ Firewall ports opened for CloudXR"
```

### **4. Configure Router Port Forwarding**
```
Manual Step: Access your router admin panel
1. Navigate to Port Forwarding/Virtual Servers
2. Add these rules:
   - Port 8080 TCP → Your local IP:8080
   - Port 7777 TCP → Your local IP:7777
   - Ports 48010-48200 TCP/UDP → Your local IP:48010-48200
```

### **5. Start CX Language Streaming**
```powershell
# Set production environment
$env:ASPNETCORE_ENVIRONMENT="Production"

# Start consciousness streaming to public IP
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/visualization/cloudxr_consciousness_stream.cx

# Your consciousness stream is now available at: ws://YOUR_PUBLIC_IP:8080/cloudxr
```

## 🎮 Client Setup (Unreal Engine)

### **1. Update Unreal Engine Connection**
In your Unreal Engine project, update the CloudXR connection:

```cpp
// In Blueprint or C++
CloudXRConfig.WebSocketURL = "ws://YOUR_PUBLIC_IP:8080/cloudxr";
CloudXRConfig.ServerAddress = "YOUR_PUBLIC_IP";
CloudXRConfig.ServerPort = 8080;
CloudXRConfig.ApiKey = "consciousness-stream-2025-secure-key";
```

### **2. Launch Unreal Engine Project**
```
1. Open ConsciousnessXR.uproject
2. Press Play in VR Preview
3. Connect VR headset to CloudXR client
4. Enter server address: YOUR_PUBLIC_IP:8080
```

## ✅ Quick Test

### **Test Connection**
```powershell
# Test from client machine
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 8080

# Should return: TcpTestSucceeded : True
```

### **Test Consciousness Streaming**
```powershell
# Check consciousness data is streaming
curl -H "X-API-Key: consciousness-stream-2025-secure-key" http://YOUR_PUBLIC_IP:8080/health

# Should return JSON with streaming status
```

## 🔧 Troubleshooting

### **Connection Issues**
```powershell
# Check if ports are open
netstat -an | findstr :8080
netstat -an | findstr :7777

# Check firewall status
Get-NetFirewallRule -DisplayName "*CloudXR*" | Select-Object DisplayName,Enabled,Direction
```

### **Performance Issues**
- Reduce MaxEntities to 25 in configuration
- Increase CompressionLevel to 0.8
- Use wired internet connection (not WiFi)
- Ensure minimum 50 Mbps upload speed

### **Security Notes**
- Change the API key in appsettings.Production.json
- Consider enabling SSL for production use
- Restrict AllowedOrigins to specific client IPs

---

*🌐 Your consciousness visualization is now streaming globally via CloudXR!*
