using CxLanguage.Core.Symbols;
using CxLanguage.Core.Types;

namespace CxLanguage.Core.Modules;

/// <summary>
/// Interface for resolving module imports at compile time
/// </summary>
public interface IModuleResolver
{
    /// <summary>
    /// Resolve a module path to exported symbols
    /// </summary>
    /// <param name="modulePath">Module path like "azure-openai" or "./my-module"</param>
    /// <returns>Dictionary of exported symbol names and their types</returns>
    ModuleResolutionResult ResolveModule(string modulePath);
    
    /// <summary>
    /// Check if a module path is supported
    /// </summary>
    bool SupportsModule(string modulePath);
}

/// <summary>
/// Result of module resolution
/// </summary>
public class ModuleResolutionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, CxType> ExportedSymbols { get; init; } = new();
    public ModuleType ModuleType { get; init; }
    public string? RuntimeTypeName { get; init; } // For service injection
    
    public static ModuleResolutionResult Success(Dictionary<string, CxType> symbols, ModuleType moduleType, string? runtimeTypeName = null) =>
        new() { IsSuccess = true, ExportedSymbols = symbols, ModuleType = moduleType, RuntimeTypeName = runtimeTypeName };
    
    public static ModuleResolutionResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Types of modules
/// </summary>
public enum ModuleType
{
    /// <summary>
    /// Built-in Azure service (azure-openai, azure-storage, etc.)
    /// </summary>
    AzureService,
    
    /// <summary>
    /// Local Cx file module
    /// </summary>
    CxModule,
    
    /// <summary>
    /// .NET assembly/library
    /// </summary>
    DotNetAssembly
}
