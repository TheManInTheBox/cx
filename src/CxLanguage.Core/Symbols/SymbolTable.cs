using CxLanguage.Core.Ast;
using CxLanguage.Core.Types;

namespace CxLanguage.Core.Symbols;

/// <summary>
/// Symbol table for managing variable and function scopes
/// </summary>
public class SymbolTable
{
    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly SymbolTable? _parent;

    public SymbolTable(SymbolTable? parent = null)
    {
        _parent = parent;
    }

    public bool TryDefine(Symbol symbol)
    {
        if (_symbols.ContainsKey(symbol.Name))
            return false;

        _symbols[symbol.Name] = symbol;
        return true;
    }

    public Symbol? Lookup(string name)
    {
        if (_symbols.TryGetValue(name, out var symbol))
            return symbol;

        return _parent?.Lookup(name);
    }

    public bool IsDefined(string name)
    {
        return _symbols.ContainsKey(name) || (_parent?.IsDefined(name) ?? false);
    }

    public SymbolTable CreateChild()
    {
        return new SymbolTable(this);
    }
}

/// <summary>
/// Base class for all symbols
/// </summary>
public abstract class Symbol
{
    public string Name { get; }
    public CxType Type { get; }
    public SymbolKind Kind { get; }

    protected Symbol(string name, CxType type, SymbolKind kind)
    {
        Name = name;
        Type = type;
        Kind = kind;
    }
}

/// <summary>
/// Variable symbol
/// </summary>
public class VariableSymbol : Symbol
{
    public bool IsMutable { get; }

    public VariableSymbol(string name, CxType type, bool isMutable = true)
        : base(name, type, SymbolKind.Variable)
    {
        IsMutable = isMutable;
    }
}

/// <summary>
/// Function symbol
/// </summary>
public class FunctionSymbol : Symbol
{
    public List<ParameterSymbol> Parameters { get; }
    public bool IsAsync { get; }
    public FunctionDeclarationNode? Declaration { get; }

    public FunctionSymbol(string name, CxType returnType, List<ParameterSymbol> parameters, bool isAsync = false, FunctionDeclarationNode? declaration = null)
        : base(name, CxType.Function(returnType, parameters.Select(p => p.Type).ToArray()), SymbolKind.Function)
    {
        Parameters = parameters;
        IsAsync = isAsync;
        Declaration = declaration;
    }
}

/// <summary>
/// Parameter symbol
/// </summary>
public class ParameterSymbol : Symbol
{
    public ParameterSymbol(string name, CxType type)
        : base(name, type, SymbolKind.Parameter)
    {
    }
}

/// <summary>
/// Import symbol for AI modules
/// </summary>
public class ImportSymbol : Symbol
{
    public string ModulePath { get; }
    public Dictionary<string, Symbol> ExportedSymbols { get; } = new();

    public ImportSymbol(string name, string modulePath)
        : base(name, CxType.Object, SymbolKind.Import)
    {
        ModulePath = modulePath;
    }
}

/// <summary>
/// AI service symbol for Azure AI integrations
/// </summary>
public class AiServiceSymbol : Symbol
{
    public string ServiceType { get; }
    public Dictionary<string, object> Configuration { get; }

    public AiServiceSymbol(string name, string serviceType, Dictionary<string, object> configuration)
        : base(name, CxType.Object, SymbolKind.AiService)
    {
        ServiceType = serviceType;
        Configuration = configuration;
    }
}

/// <summary>
/// Symbol kinds
/// </summary>
public enum SymbolKind
{
    Variable,
    Function,
    Parameter,
    Import,
    AiService,
    Type
}
