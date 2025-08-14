# CX Language Build Guide

## � **WORKING FEATURES CONFIRMED**

✅ **Local LLM Integration** - 2GB Llama model loading and inference  
✅ **Mathematical Problem Solving** - AI agents solving 23+23 with step-by-step solutions  
✅ **Event-Driven Architecture** - Full event bus with >10,000 events/second capability  
✅ **IL Compilation** - Three-pass .NET IL generation with consciousness awareness  
✅ **Cognitive Boolean Logic** - `is {}` and `not {}` patterns replacing traditional if-statements  
✅ **Auto JSON Serialization** - Conscious entities automatically serialize for debugging  

## �🚨 **CRITICAL: Proper Working Directories**

**ALWAYS ensure you are in the correct working directory before running commands.**

### **Project Root Directory**
```
C:\Users\aaron\Source\cx\
```

**Verify you're in the right place:**
```powershell
pwd
# Should output: C:\Users\aaron\Source\cx
```

**If not in project root, navigate there:**
```powershell
cd C:\Users\aaron\Source\cx
```

---

## 🔨 **Build Commands (From Project Root)**

### **1. Full Solution Build**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet build CxLanguage.sln
```

### **2. Clean and Rebuild**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet clean CxLanguage.sln
dotnet build CxLanguage.sln
```

### **3. Build Specific Projects**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet build src/CxLanguage.Core/CxLanguage.Core.csproj
dotnet build src/CxLanguage.Parser/CxLanguage.Parser.csproj  
dotnet build src/CxLanguage.Compiler/CxLanguage.Compiler.csproj
dotnet build src/CxLanguage.Runtime/CxLanguage.Runtime.csproj
dotnet build src/CxLanguage.StandardLibrary/CxLanguage.StandardLibrary.csproj
dotnet build src/CxLanguage.CLI/CxLanguage.CLI.csproj
```

---

## 🏃‍♂️ **Run Commands (From Project Root)**

### **1. Run CX Scripts**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/script_name.cx
```

### **2. Voice Input Testing**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/voice_bridge_test.cx
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/voice_input_working_test.cx
```

### **3. Debug with Verbose Output**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/script_name.cx 2>&1
```

---

## 📁 **Key Directory Structure**

```
C:\Users\aaron\Source\cx\                    ← PROJECT ROOT (WORK FROM HERE)
├── CxLanguage.sln                          ← Main solution file
├── BUILD_GUIDE.md                          ← This file
├── src/
│   ├── CxLanguage.Core/                    ← AST, interfaces, core types
│   ├── CxLanguage.Parser/                  ← ANTLR grammar, parsing
│   ├── CxLanguage.Compiler/                ← IL generation, compilation
│   ├── CxLanguage.Runtime/                 ← Event bus, runtime helpers
│   ├── CxLanguage.StandardLibrary/         ← AI services, event bridges
│   └── CxLanguage.CLI/                     ← Command-line interface
├── examples/                               ← CX language programs
├── grammar/                                ← ANTLR grammar files
├── tests/                                  ← Unit tests
└── wiki/                                   ← Documentation
```

---

## ❌ **Common Path Errors**

### **Problem: Working from wrong directory**
```powershell
# ❌ WRONG - From CLI subdirectory
C:\Users\aaron\Source\cx\src\CxLanguage.CLI> dotnet build CxLanguage.sln
# Error: Project file does not exist

# ✅ CORRECT - From project root
C:\Users\aaron\Source\cx> dotnet build CxLanguage.sln
```

### **Problem: Incorrect relative paths**
```powershell
# ❌ WRONG - Incorrect CLI path
dotnet run --project ./src/CxLanguage.CLI/CxLanguage.CLI.csproj

# ✅ CORRECT - Proper CLI path from root
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj
```

---

## 🎯 **Quick Start Workflow**

### **1. Open Terminal in Project Root**
```powershell
cd C:\Users\aaron\Source\cx
pwd  # Verify: C:\Users\aaron\Source\cx
```

### **2. Build Solution**
```powershell
dotnet build CxLanguage.sln
```

### **3. Test Voice Input System**
```powershell
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/voice_bridge_test.cx
```

---

## 🔧 **Development Commands**

### **Grammar Regeneration (After grammar changes)**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet build src/CxLanguage.Parser/CxLanguage.Parser.csproj
```

### **Clean Build (After major changes)**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet clean CxLanguage.sln
dotnet build CxLanguage.sln
```

### **Run All Tests**
```powershell
# From: C:\Users\aaron\Source\cx\
dotnet test
```

---

## 🚨 **Emergency Build Fixes**

### **If build fails with path errors:**
1. **Verify working directory**: `pwd`
2. **Navigate to project root**: `cd C:\Users\aaron\Source\cx`
3. **Clean and rebuild**: `dotnet clean CxLanguage.sln && dotnet build CxLanguage.sln`

### **If CLI commands fail:**
1. **Check project root**: `pwd` should show `C:\Users\aaron\Source\cx`
2. **Verify CLI project exists**: `ls src/CxLanguage.CLI/CxLanguage.CLI.csproj`
3. **Use full path**: `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test.cx`

---

## 💡 **Pro Tips**

1. **Always start from project root**: `C:\Users\aaron\Source\cx\`
2. **Use tab completion**: `dotnet run --project src/Cx[TAB]`
3. **Create aliases** for common commands:
   ```powershell
   # In PowerShell profile
   function cxbuild { cd C:\Users\aaron\Source\cx; dotnet build CxLanguage.sln }
   function cxrun { cd C:\Users\aaron\Source\cx; dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run $args }
   ```

---

**📝 Last Updated**: July 24, 2025  
**👥 Team**: Keep this document updated when directory structure changes!
