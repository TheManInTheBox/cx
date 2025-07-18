# Mock Services Configuration for CI/CD

## Overview
This document describes the mock services configuration used during CI/CD builds and tests to ensure the CX Language can be tested without requiring live external services.

## Mock Services

### Azure OpenAI Mock
- **Environment Variable**: `AZURE_OPENAI_MOCK=true`
- **Purpose**: Mocks Azure OpenAI API responses for AI function testing
- **Implementation**: Returns predefined responses for all AI functions:
  - `task()` - Returns "Mock task result"
  - `reason()` - Returns "Mock reasoning result"
  - `synthesize()` - Returns "Mock synthesized content"
  - `process()` - Returns "Mock processed data"
  - `generate()` - Returns "Mock generated content"
  - `embed()` - Returns mock vector embedding array
  - `adapt()` - Returns mock code or performs mock function injection

### Application Insights Mock
- **Environment Variable**: `APPLICATION_INSIGHTS_MOCK=true`
- **Purpose**: Mocks Application Insights telemetry collection
- **Implementation**: No-op telemetry collection that doesn't send data to Azure

### Mock Configuration Detection
The CX Language runtime detects mock configuration through:
1. Environment variables set during CI/CD
2. Configuration flags in `appsettings.json`
3. Conditional compilation directives for debug builds

## CI/CD Configuration

### Debug Build (Default)
- **Configuration**: Debug
- **Mock Services**: Enabled by default
- **Purpose**: Full feature testing without external dependencies
- **Environment Variables**:
  - `USE_MOCK_SERVICES=true`
  - `AZURE_OPENAI_MOCK=true`
  - `APPLICATION_INSIGHTS_MOCK=true`

### Release Build
- **Configuration**: Release
- **Mock Services**: Disabled (uses live services when configured)
- **Purpose**: Production-ready build validation
- **Requirements**: Live Azure OpenAI configuration for full AI testing

## Mock Implementation Strategy

### 1. Azure OpenAI Mock Implementation
```csharp
// In SemanticKernelAiFunctions.cs
public class SemanticKernelAiFunctions
{
    private readonly bool _useMockServices;
    
    public SemanticKernelAiFunctions(IConfiguration configuration)
    {
        _useMockServices = configuration.GetValue<bool>("USE_MOCK_SERVICES") ||
                          Environment.GetEnvironmentVariable("AZURE_OPENAI_MOCK") == "true";
    }
    
    public async Task<string> TaskAsync(string prompt, object options = null)
    {
        if (_useMockServices)
        {
            return "Mock task result for: " + prompt;
        }
        // Real implementation
        return await _actualAzureOpenAIService.TaskAsync(prompt, options);
    }
    
    // Similar mock implementations for all AI functions
}
```

### 2. Runtime Function Injection Mock
```csharp
public async Task<string> AdaptAsync(string prompt, object options = null)
{
    if (_useMockServices)
    {
        // Mock function injection
        if (IsRuntimeFunctionInjectionRequest(options))
        {
            return MockGenerateAndInjectFunction(prompt, options);
        }
        return "Mock adapted content for: " + prompt;
    }
    // Real implementation
    return await _actualAzureOpenAIService.AdaptAsync(prompt, options);
}

private string MockGenerateAndInjectFunction(string prompt, object options)
{
    // Generate mock CX function code
    var mockCode = GenerateMockCxFunction(options);
    
    // Compile and inject (same as real implementation)
    return CompileAndInjectCxCodeAsync(mockCode);
}
```

### 3. Application Insights Mock
```csharp
public class MockTelemetryService : ITelemetryService
{
    public void TrackEvent(string eventName, IDictionary<string, string> properties = null)
    {
        // No-op for CI/CD
        Console.WriteLine($"Mock Telemetry: {eventName}");
    }
    
    public void TrackException(Exception exception)
    {
        // No-op for CI/CD
        Console.WriteLine($"Mock Exception: {exception.Message}");
    }
}
```

## Testing Strategy

### 1. Core Language Features
- **Tested**: All Phase 1-3 features (variables, functions, control flow)
- **Mock Requirement**: None (no external dependencies)
- **Status**: ✅ Fully tested

### 2. AI Integration (Phase 4)
- **Tested**: All AI function syntax, parsing, and compilation
- **Mock Requirement**: Azure OpenAI mock for function execution
- **Status**: ✅ Fully tested with mocks

### 3. Runtime Function Injection
- **Tested**: Complete workflow with mock AI-generated code
- **Mock Requirement**: Mock code generation instead of live AI
- **Status**: ✅ Fully tested with mocks

## Benefits of Mock Services

1. **Reliability**: Tests don't depend on external service availability
2. **Speed**: Mock responses are instant, faster CI/CD pipelines
3. **Cost**: No Azure OpenAI API charges during testing
4. **Isolation**: Tests focus on CX Language functionality, not external services
5. **Determinism**: Consistent mock responses enable reliable testing

## Production Deployment

For production deployment:
1. Set `USE_MOCK_SERVICES=false`
2. Configure live Azure OpenAI credentials
3. Configure Application Insights connection string
4. Run release build with live service integration

## Mock Service Files

- `src/CxLanguage.Core/Services/MockAzureOpenAIService.cs`
- `src/CxLanguage.Core/Services/MockTelemetryService.cs`
- `src/CxLanguage.Core/Services/MockServiceRegistration.cs`
- `appsettings.Mock.json` - Mock service configuration

## Environment Variables Summary

```bash
# Enable all mock services
USE_MOCK_SERVICES=true

# Individual service mocks
AZURE_OPENAI_MOCK=true
APPLICATION_INSIGHTS_MOCK=true

# CI/CD specific
DOTNET_ENVIRONMENT=Development
CONFIGURATION=Debug
```

This mock service configuration ensures that the CX Language can be fully tested in CI/CD environments without requiring live Azure services, while maintaining the ability to test against real services when needed.
