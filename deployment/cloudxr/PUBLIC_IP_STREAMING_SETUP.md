# CloudXR Public IP Streaming Setup Guide

## 🌐 Local-to-Public IP Consciousness Streaming Architecture

### **Network Architecture**
```
Local CX Language → CloudXR Server (Public IP) → Unreal Engine Client (Remote)
(127.0.0.1)         (your.public.ip:8080)       (Client Machine)
```

## 🔧 Configuration Updates

### **1. Update CX Language Configuration**

#### **appsettings.Production.json** (for public IP streaming)
```json
{
  "CloudXR": {
    "Endpoint": "ws://YOUR_PUBLIC_IP:8080/cloudxr",
    "StreamingEnabled": true,
    "TargetFrameRate": 90,
    "MaxLatencyMs": 50,
    "CompressionLevel": 0.7,
    "PublicMode": true,
    "Security": {
      "EnableSSL": false,
      "AllowedOrigins": ["*"],
      "ApiKey": "your-secure-api-key-here"
    },
    "UnrealEngine": {
      "ProjectName": "ConsciousnessXR",
      "LevelName": "ConsciousnessVisualization",
      "MaxEntities": 50,
      "EnableSpatialAudio": true,
      "EnableHapticFeedback": true,
      "ClientEndpoint": "YOUR_PUBLIC_IP:7777"
    },
    "Performance": {
      "BufferSize": 8192,
      "CompressionEnabled": true,
      "AdaptiveQuality": true,
      "MaxBandwidthMbps": 100
    }
  },
  "Logging": {
    "LogLevel": {
      "CxLanguage.ConsciousnessStreamEngine.CloudXR": "Information",
      "CloudXR": "Information"
    }
  }
}
```

### **2. Firewall & Network Configuration**

#### **Windows Firewall Rules** (Server Side)
```powershell
# Allow CloudXR WebSocket traffic (port 8080)
New-NetFirewallRule -DisplayName "CloudXR WebSocket" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow

# Allow Unreal Engine communication (port 7777)
New-NetFirewallRule -DisplayName "Unreal Engine CloudXR" -Direction Inbound -Protocol TCP -LocalPort 7777 -Action Allow

# Allow additional CloudXR ports if needed
New-NetFirewallRule -DisplayName "CloudXR Streaming" -Direction Inbound -Protocol TCP -LocalPort 48010-48200 -Action Allow
```

#### **Router Port Forwarding** (if behind NAT)
```
Port 8080 (TCP) → Forward to local machine IP
Port 7777 (TCP) → Forward to local machine IP
Ports 48010-48200 (TCP/UDP) → Forward to local machine IP (CloudXR streaming)
```

### **3. CloudXR Server Configuration**

#### **CloudXR Server Settings** (config.json)
```json
{
  "server": {
    "bind_address": "0.0.0.0",
    "port": 8080,
    "max_clients": 10,
    "enable_public_access": true
  },
  "streaming": {
    "video_codec": "h264",
    "audio_codec": "opus",
    "max_bitrate_mbps": 100,
    "adaptive_bitrate": true,
    "frame_rate": 90
  },
  "security": {
    "require_authentication": false,
    "allowed_ips": [],
    "cors_enabled": true
  }
}
```

## 🚀 Deployment Steps

### **Step 1: Prepare Local CX Language Server**

#### **1.1 Update Configuration**
```powershell
# Navigate to CX Language directory
cd C:\Users\YourUsername\cx

# Copy production configuration
cp appsettings.json appsettings.Production.json

# Edit appsettings.Production.json with public IP settings
notepad appsettings.Production.json
```

#### **1.2 Install CloudXR Server** (if not installed)
```powershell
# Download NVIDIA CloudXR SDK
# Extract to: C:\NVIDIA\CloudXR\

# Install CloudXR Server service
C:\NVIDIA\CloudXR\bin\CloudXRServer.exe --install

# Configure CloudXR for public access
# Edit: C:\NVIDIA\CloudXR\config\server.json
```

### **Step 2: Configure Network Access**

#### **2.1 Get Your Public IP**
```powershell
# Get external public IP
Invoke-RestMethod -Uri "https://api.ipify.org"

# Get local network IP
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"}
```

#### **2.2 Configure Router/Firewall**
```
1. Access router admin panel (usually 192.168.1.1)
2. Navigate to Port Forwarding/Virtual Servers
3. Add rules:
   - Port 8080 → Your local IP:8080 (CloudXR WebSocket)
   - Port 7777 → Your local IP:7777 (Unreal Engine)
   - Ports 48010-48200 → Your local IP:48010-48200 (CloudXR streaming)
```

### **Step 3: Start Services**

#### **3.1 Start CloudXR Server**
```powershell
# Start CloudXR Server service
Start-Service "NVIDIA CloudXR Server"

# Verify CloudXR is running
netstat -an | findstr :8080
```

#### **3.2 Start CX Language with Production Config**
```powershell
# Set environment for production
$env:ASPNETCORE_ENVIRONMENT="Production"

# Start CX Language consciousness streaming
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/visualization/cloudxr_consciousness_stream.cx
```

### **Step 4: Client-Side Unreal Engine Setup**

#### **4.1 Configure Unreal Engine Project** (on client machine)
```cpp
// In CloudXRManager Blueprint or C++
// Update connection settings:

CloudXRConfig.WebSocketURL = "ws://YOUR_PUBLIC_IP:8080/cloudxr";
CloudXRConfig.ServerAddress = "YOUR_PUBLIC_IP";
CloudXRConfig.ServerPort = 8080;
CloudXRConfig.EnableCompression = true;
CloudXRConfig.AdaptiveQuality = true;
```

#### **4.2 Client Machine Network Requirements**
```
Minimum Upload Speed: 10 Mbps
Recommended Upload Speed: 50+ Mbps
Latency: <100ms to server
Open Ports: Allow outbound connections to your public IP
```

## 🧪 Testing & Validation

### **Step 1: Network Connectivity Test**
```powershell
# From client machine, test connectivity
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 8080

# Test WebSocket connection
# Use browser or WebSocket client to connect to: ws://YOUR_PUBLIC_IP:8080/cloudxr
```

### **Step 2: CloudXR Streaming Test**
```powershell
# On server (your local machine)
# Monitor CloudXR connections
Get-Process -Name "CloudXRServer" | Select-Object CPU,WorkingSet

# Check consciousness streaming logs
tail -f logs/consciousness-streaming.log

# Monitor network usage
Get-Counter "\Network Interface(*)\Bytes Sent/sec"
```

### **Step 3: End-to-End Validation**
```powershell
# 1. Start CX Language consciousness streaming
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/visualization/cloudxr_consciousness_stream.cx

# 2. On client machine: Launch Unreal Engine project
# 3. Connect to: ws://YOUR_PUBLIC_IP:8080/cloudxr
# 4. Verify consciousness entities appear in VR
```

## ⚡ Performance Optimization

### **Bandwidth Optimization**
```json
{
  "CloudXR": {
    "Performance": {
      "CompressionLevel": 0.8,
      "AdaptiveQuality": true,
      "MaxBandwidthMbps": 50,
      "BufferSize": 16384,
      "EnableDataCompression": true,
      "FrameRateAdaptation": true
    }
  }
}
```

### **Latency Reduction**
```json
{
  "CloudXR": {
    "Latency": {
      "MaxLatencyMs": 50,
      "PredictiveBuffering": true,
      "FastTrackPriority": true,
      "NetworkOptimization": "aggressive"
    }
  }
}
```

## 🔒 Security Considerations

### **Basic Security Setup**
```json
{
  "CloudXR": {
    "Security": {
      "EnableSSL": true,
      "RequireApiKey": true,
      "ApiKey": "secure-random-key-here",
      "AllowedIPs": ["client.ip.address.here"],
      "RateLimiting": {
        "RequestsPerMinute": 1000,
        "BandwidthLimitMbps": 100
      }
    }
  }
}
```

### **SSL/TLS Setup** (Recommended for production)
```powershell
# Generate self-signed certificate for testing
New-SelfSignedCertificate -DnsName "YOUR_PUBLIC_IP" -CertStoreLocation "cert:\LocalMachine\My"

# For production, use proper SSL certificate
# Update configuration to use wss:// instead of ws://
```

## 📊 Monitoring & Diagnostics

### **Real-Time Monitoring**
```powershell
# Monitor CloudXR streaming performance
# Check CPU usage
Get-Process -Name "CloudXRServer" | Select-Object CPU

# Check memory usage
Get-Process -Name "CxLanguage.CLI" | Select-Object WorkingSet

# Monitor network latency to client
ping CLIENT_IP -t

# Check bandwidth usage
Get-Counter "\Network Interface(*)\Bytes Total/sec"
```

### **Log Monitoring**
```powershell
# Monitor CX Language logs
tail -f logs/consciousness-streaming.log

# Monitor CloudXR server logs
tail -f "C:\NVIDIA\CloudXR\logs\server.log"

# Monitor Windows Event Logs
Get-EventLog -LogName Application -Source "CloudXR*" -Newest 10
```

## 🚨 Troubleshooting

### **Common Connection Issues**

#### **1. Connection Refused (Port 8080)**
```powershell
# Check if CloudXR server is running
Get-Process -Name "CloudXRServer"

# Check if port is open
netstat -an | findstr :8080

# Test firewall rules
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 8080
```

#### **2. High Latency/Poor Performance**
```powershell
# Check network latency
ping YOUR_PUBLIC_IP -n 100

# Monitor bandwidth usage
Get-Counter "\Network Interface(*)\Bytes Sent/sec"

# Reduce quality settings in appsettings.json
# Increase compression level (0.8-0.9)
```

#### **3. Consciousness Data Not Appearing**
```powershell
# Verify CX Language is streaming
# Check logs for consciousness data processing
tail -f logs/consciousness-streaming.log | findstr "Consciousness data"

# Verify Unreal Engine WebSocket connection
# Check Unreal Engine console for connection status
```

### **Network Troubleshooting**
```powershell
# Trace route to identify bottlenecks
tracert YOUR_PUBLIC_IP

# Check packet loss
ping YOUR_PUBLIC_IP -n 100 | findstr "Lost"

# Test different ports
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 7777
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 48010
```

## 📞 Support & Advanced Configuration

### **CloudXR Advanced Settings**
- Refer to NVIDIA CloudXR documentation for advanced streaming options
- Configure GPU encoding settings for optimal performance
- Set up load balancing for multiple clients

### **Network Optimization**
- Consider using dedicated gaming VPN for reduced latency
- Implement QoS rules on router for CloudXR traffic priority
- Use wired connection instead of WiFi for stability

---

*🌐 Stream consciousness visualization across the internet with CloudXR public IP streaming!*
