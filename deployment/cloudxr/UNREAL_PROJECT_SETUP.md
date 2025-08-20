# Unreal Engine CloudXR Consciousness Project Setup Guide

## 📋 Prerequisites

### Required Software
- **Unreal Engine 5.3+** (Download from Epic Games Launcher)
- **Visual Studio 2022** (with C++ development tools)
- **NVIDIA CloudXR SDK 4.0+** (Download from NVIDIA Developer)
- **Git** (for project version control)

### Hardware Requirements
- **GPU**: RTX 3070+ with 8GB+ VRAM
- **RAM**: 32GB+ recommended
- **Storage**: 50GB+ free space for Unreal project
- **VR Headset**: Quest 2/3, Pico 4, or PCVR headset

## 🚀 Step 1: Create New Unreal Project

### 1.1 Launch Unreal Engine 5.3+
```
1. Open Epic Games Launcher
2. Go to "Unreal Engine" → "Library"
3. Click "Launch" on Unreal Engine 5.3+
```

### 1.2 Create VR Project
```
1. Select "Games" category
2. Choose "Virtual Reality" template
3. Configure project settings:
   - Project Name: "ConsciousnessXR"
   - Location: "C:\UE5_Projects\ConsciousnessXR"
   - Blueprint: Yes (recommended for rapid prototyping)
   - Target Platform: Desktop
   - Quality Preset: Maximum
   - Raytracing: Enabled (if RTX GPU available)
4. Click "Create"
```

## 🔌 Step 2: Install CloudXR Plugin

### 2.1 Download CloudXR SDK
```
1. Visit: https://developer.nvidia.com/cloudxr-sdk
2. Download CloudXR SDK 4.0+
3. Extract to: C:\NVIDIA\CloudXR\
```

### 2.2 Add CloudXR Plugin to Project
```
1. Copy CloudXR Unreal plugin:
   Source: C:\NVIDIA\CloudXR\UnrealEngine\CloudXRPlugin\
   Destination: C:\UE5_Projects\ConsciousnessXR\Plugins\CloudXRPlugin\

2. Enable plugin in Unreal:
   - Edit → Plugins
   - Search "CloudXR"
   - Enable "NVIDIA CloudXR" plugin
   - Restart Unreal Engine
```

## 🧠 Step 3: Import Consciousness Blueprints

### 3.1 Create Consciousness Blueprint Library
```
1. In Content Browser, create folders:
   - Content/Consciousness/
   - Content/Consciousness/Blueprints/
   - Content/Consciousness/Materials/
   - Content/Consciousness/Particles/
   - Content/Consciousness/Audio/
```

### 3.2 Create Consciousness Entity Blueprint

#### A. Create ConsciousnessEntity Blueprint
```
1. Right-click in Content/Consciousness/Blueprints/
2. Blueprint Class → Actor
3. Name: "BP_ConsciousnessEntity"
4. Open the blueprint
```

#### B. Add Components
```
In Components panel, add:
1. Static Mesh Component (Root)
   - Name: "ConsciousnessMesh"
   - Static Mesh: Engine/BasicShapes/Sphere

2. Niagara Component
   - Name: "AuraParticles"
   - Attach to: ConsciousnessMesh

3. Audio Component
   - Name: "ConsciousnessAudio"
   - Attach to: ConsciousnessMesh

4. Widget Component (for debugging)
   - Name: "DebugWidget"
   - Attach to: ConsciousnessMesh
```

#### C. Add Blueprint Variables
```
In Variables panel, add:
1. ConsciousnessData (Struct)
   - Type: ConsciousnessData (create custom struct)
   - Default Value: Awareness=0.5, Emotion=0.5, Energy=0.5

2. AuraRadius (Float)
   - Default: 5.0
   - Min: 1.0, Max: 20.0

3. IsActive (Boolean)
   - Default: true
```

### 3.3 Create Consciousness Data Structure

#### A. Create ConsciousnessData Struct
```
1. Right-click in Content/Consciousness/Blueprints/
2. Blueprint → Structure
3. Name: "ST_ConsciousnessData"
4. Add variables:
   - Location (Vector)
   - Awareness (Float 0-1)
   - Emotion (Float 0-1)  
   - Energy (Float 0-1)
   - ConsciousnessLevel (Float 0-1)
```

#### B. Create Consciousness Flow Blueprint
```
1. Blueprint Class → Actor
2. Name: "BP_ConsciousnessFlow"
3. Add components:
   - Spline Component (Root)
   - Niagara Component (particle trail)
   - Audio Component (flow sounds)
```

## 🎨 Step 4: Create Consciousness Materials

### 4.1 Consciousness Entity Material
```
1. Right-click in Content/Consciousness/Materials/
2. Material
3. Name: "M_ConsciousnessEntity"
4. Create material graph:
   - Base Color: Connected to ConsciousnessLevel parameter
   - Emissive: Connected to Energy parameter  
   - Opacity: Connected to Awareness parameter
   - Add fresnel effect for aura glow
```

### 4.2 Consciousness Aura Material
```
1. Material
2. Name: "M_ConsciousnessAura"
3. Material Domain: Volume
4. Create volumetric material for aura effects
```

## 💫 Step 5: Create Particle Systems

### 5.1 Consciousness Aura Particles
```
1. Right-click in Content/Consciousness/Particles/
2. FX → Niagara System
3. Template: Simple Sphere Location
4. Name: "NS_ConsciousnessAura"
5. Configure:
   - Particle count based on Energy level
   - Color based on Emotion state
   - Size based on Awareness level
```

### 5.2 Consciousness Flow Particles
```
1. Niagara System
2. Template: Ribbon Template
3. Name: "NS_ConsciousnessFlow"
4. Configure for spline-based particle trails
```

## 🔗 Step 6: Setup CloudXR Integration

### 6.1 Create CloudXR Manager Blueprint
```
1. Blueprint Class → Actor
2. Name: "BP_CloudXRManager"
3. Add components:
   - CloudXR Component
   - WebSocket Component (for CX Language connection)
```

### 6.2 Create WebSocket Connection
```
In BP_CloudXRManager Event Graph:

1. Begin Play Event:
   - Connect to WebSocket: ws://127.0.0.1:8080/cloudxr
   - Set CloudXR streaming parameters

2. Custom Event: "ReceiveConsciousnessData"
   - Parse JSON consciousness data
   - Update consciousness entities
   - Trigger visual effects

3. Custom Function: "UpdateConsciousnessEntity"
   - Input: Entity ID, Consciousness Data
   - Update entity position, aura, particles
```

### 6.3 JSON Parsing Setup
```
In Project Settings:
1. Enable JSON Blueprint plugin
2. Add JSON parsing nodes for consciousness data
3. Map CX Language data to Unreal structures
```

## 🎮 Step 7: Setup VR Integration

### 7.1 Configure VR Settings
```
Project Settings → XR:
1. Start in VR: Enabled
2. Support VR Headsets: All supported headsets
3. Instanced Stereo: Enabled
4. Mobile Multi-View: Enabled (for Quest)
```

### 7.2 Create VR Pawn
```
1. Blueprint Class → Pawn
2. Name: "BP_VRConsciousnessPawn"
3. Add VR components:
   - VR Camera
   - Motion Controllers (Left/Right)
   - Teleportation component
   - Hand tracking (if supported)
```

### 7.3 VR Interaction with Consciousness
```
Add VR interaction blueprints:
1. Grab consciousness entities
2. Manipulate consciousness flows
3. Adjust consciousness parameters via hand gestures
4. Spatial audio for consciousness entities
```

## 🌐 Step 8: Network Configuration

### 8.1 Configure WebSocket Settings
```
In BP_CloudXRManager:
1. WebSocket URL: ws://127.0.0.1:8080/cloudxr
2. Auto-reconnect: Enabled
3. Heartbeat interval: 5 seconds
4. Message queue size: 1000
```

### 8.2 Create Consciousness Data Handler
```
Custom Events:
1. OnWebSocketConnected
2. OnWebSocketMessage (parse consciousness data)
3. OnWebSocketDisconnected
4. OnConsciousnessUpdate (update entities)
```

## 🧪 Step 9: Testing Setup

### 9.1 Create Test Level
```
1. File → New Level
2. Choose "Empty Level"
3. Save as: "ConsciousnessVisualization"
4. Add basic lighting and environment
```

### 9.2 Add Test Consciousness Entities
```
1. Drag BP_ConsciousnessEntity into level
2. Set test consciousness data
3. Add BP_CloudXRManager to level
4. Configure test WebSocket connection
```

### 9.3 Package for Testing
```
1. File → Package Project → Windows (64-bit)
2. Choose output directory
3. Test with VR headset connected
4. Verify CloudXR streaming
```

## 🔧 Step 10: Project Configuration Files

### 10.1 DefaultEngine.ini Settings
```ini
[/Script/Engine.Engine]
+ActiveGameNameRedirects=(OldGameName="TP_VR",NewGameName="/Script/ConsciousnessXR")

[/Script/WindowsTargetPlatform.WindowsTargetSettings]
DefaultGraphicsRHI=DefaultGraphicsRHI_DX12

[/Script/Engine.RendererSettings]
r.VRS.Enable=1
r.VR.InstancedStereo=1

[CloudXR]
StreamingEnabled=true
TargetFrameRate=90
MaxLatencyMs=15
```

### 10.2 DefaultGame.ini Settings
```ini
[/Script/EngineSettings.GeneralProjectSettings]
ProjectID=ConsciousnessXR
ProjectName=Consciousness XR Visualization
Description=Immersive consciousness data visualization via CloudXR

[/Script/Engine.NetworkSettings]
n.VerifyPeer=false
```

## 🚀 Launch Instructions

### Development Launch
```powershell
# 1. Start CX Language consciousness streaming
cd C:\Users\YourUsername\cx
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/visualization/cloudxr_consciousness_stream.cx

# 2. Launch Unreal project
# Open ConsciousnessXR.uproject
# Press Play in VR Preview

# 3. Connect VR headset
# Launch CloudXR client on VR headset
# Connect to: 127.0.0.1:8080
```

### Production Launch
```powershell
# 1. Package Unreal project
# 2. Deploy to target machine with GPU
# 3. Configure CloudXR server
# 4. Launch consciousness streaming service
# 5. Connect VR clients
```

## 📚 Additional Resources

### Unreal Engine Documentation
- [VR Development](https://docs.unrealengine.com/5.3/en-US/virtual-reality-development-in-unreal-engine/)
- [Niagara Particles](https://docs.unrealengine.com/5.3/en-US/niagara-visual-effects-in-unreal-engine/)
- [Blueprint Networking](https://docs.unrealengine.com/5.3/en-US/networking-and-multiplayer-in-unreal-engine/)

### CloudXR Documentation
- [CloudXR SDK Guide](https://docs.nvidia.com/cloudxr-sdk/index.html)
- [Unreal Engine Integration](https://docs.nvidia.com/cloudxr-sdk/unreal.html)

### CX Language Integration
- [Consciousness Streaming Guide](../../../docs/CONSCIOUSNESS_STREAMING.md)
- [WebSocket API Reference](../../../docs/WEBSOCKET_API.md)

---

*🎮 Transform consciousness data into immersive VR experiences with Unreal Engine 5 and CloudXR!*
