using CxLanguage.Core.Modules;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Composite module resolver that delegates to specialized resolvers
/// </summary>
public class CompositeModuleResolver : IModuleResolver
{
    private readonly List<IModuleResolver> _resolvers;

    public CompositeModuleResolver()
    {
        _resolvers = new List<IModuleResolver>
        {
            new AzureServiceModuleResolver(),
            // Future: CxModuleResolver for .cx files
            // Future: DotNetAssemblyResolver for .NET libraries
        };
    }

    public bool SupportsModule(string modulePath)
    {
        return _resolvers.Any(resolver => resolver.SupportsModule(modulePath));
    }

    public ModuleResolutionResult ResolveModule(string modulePath)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.SupportsModule(modulePath))
            {
                return resolver.ResolveModule(modulePath);
            }
        }

        return ModuleResolutionResult.Failure($"No resolver found for module: {modulePath}");
    }
}
