using CxLanguage.Core.Modules;
using CxLanguage.Core.Symbols;
using CxLanguage.Core.Types;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Resolves Azure service modules like "azure-openai", "azure-storage"
/// </summary>
public class AzureServiceModuleResolver : IModuleResolver
{
    private readonly Dictionary<string, ModuleResolutionResult> _supportedModules;

    public AzureServiceModuleResolver()
    {
        _supportedModules = new Dictionary<string, ModuleResolutionResult>
        {
            ["azure-openai"] = ModuleResolutionResult.Success(
                new Dictionary<string, CxType>
                {
                    ["generateText"] = CxType.Function(CxType.String, CxType.String),
                    ["analyze"] = CxType.Function(CxType.String, CxType.String),
                    ["streamGenerate"] = CxType.Function(CxType.Object, CxType.String),
                    ["processBatch"] = CxType.Function(CxType.Array(CxType.String), CxType.Array(CxType.String))
                },
                ModuleType.AzureService,
                "CxLanguage.Azure.Services.IAiService"
            ),
            
            ["azure-storage"] = ModuleResolutionResult.Success(
                new Dictionary<string, CxType>
                {
                    ["uploadBlob"] = CxType.Function(CxType.String, CxType.String, CxType.Object),
                    ["downloadBlob"] = CxType.Function(CxType.Object, CxType.String),
                    ["listBlobs"] = CxType.Function(CxType.Array(CxType.String)),
                    ["deleteBlob"] = CxType.Function(CxType.Boolean, CxType.String)
                },
                ModuleType.AzureService,
                "CxLanguage.Azure.Services.IAzureStorageService"
            ),
            
            ["azure-cognitive"] = ModuleResolutionResult.Success(
                new Dictionary<string, CxType>
                {
                    ["analyzeImage"] = CxType.Function(CxType.Object, CxType.String),
                    ["recognizeText"] = CxType.Function(CxType.String, CxType.String),
                    ["detectLanguage"] = CxType.Function(CxType.String, CxType.String),
                    ["translateText"] = CxType.Function(CxType.String, CxType.String, CxType.String)
                },
                ModuleType.AzureService,
                "CxLanguage.Azure.Services.IAzureCognitiveService"
            )
        };
    }

    public bool SupportsModule(string modulePath)
    {
        return _supportedModules.ContainsKey(modulePath);
    }

    public ModuleResolutionResult ResolveModule(string modulePath)
    {
        if (_supportedModules.TryGetValue(modulePath, out var result))
        {
            return result;
        }
        
        return ModuleResolutionResult.Failure($"Unknown Azure service module: {modulePath}");
    }
}
