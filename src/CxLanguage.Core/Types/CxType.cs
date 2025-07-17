namespace CxLanguage.Core.Types;

/// <summary>
/// Represents the type system for the Cx language
/// </summary>
public abstract class CxType
{
    public abstract string Name { get; }
    public abstract bool IsAssignableFrom(CxType other);

    // Built-in types
    public static readonly CxType String = new PrimitiveType("string");
    public static readonly CxType Number = new PrimitiveType("number");
    public static readonly CxType Boolean = new PrimitiveType("boolean");
    public static readonly CxType Any = new PrimitiveType("any");
    public static readonly CxType Void = new PrimitiveType("void");
    public static readonly CxType Object = new PrimitiveType("object");

    public static ArrayType Array(CxType elementType) => new(elementType);
    public static FunctionType Function(CxType returnType, params CxType[] parameters) => new(returnType, parameters);
    public static PromiseType Promise(CxType valueType) => new(valueType);

    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is CxType other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
}

/// <summary>
/// Primitive types (string, number, boolean, etc.)
/// </summary>
public class PrimitiveType : CxType
{
    public override string Name { get; }

    public PrimitiveType(string name)
    {
        Name = name;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        if (Name == "any") return true;
        if (other.Name == "any") return true;
        return Name == other.Name;
    }
}

/// <summary>
/// Array type with element type
/// </summary>
public class ArrayType : CxType
{
    public CxType ElementType { get; }
    public override string Name => $"array<{ElementType.Name}>";

    public ArrayType(CxType elementType)
    {
        ElementType = elementType;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        if (other is ArrayType arrayType)
        {
            return ElementType.IsAssignableFrom(arrayType.ElementType);
        }
        return false;
    }
}

/// <summary>
/// Function type with parameters and return type
/// </summary>
public class FunctionType : CxType
{
    public CxType ReturnType { get; }
    public CxType[] ParameterTypes { get; }
    public bool IsAsync { get; set; }

    public override string Name
    {
        get
        {
            var paramStr = string.Join(", ", ParameterTypes.Select(p => p.Name));
            var asyncPrefix = IsAsync ? "async " : "";
            return $"{asyncPrefix}({paramStr}) -> {ReturnType.Name}";
        }
    }

    public FunctionType(CxType returnType, params CxType[] parameterTypes)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        if (other is FunctionType funcType)
        {
            if (!ReturnType.IsAssignableFrom(funcType.ReturnType))
                return false;

            if (ParameterTypes.Length != funcType.ParameterTypes.Length)
                return false;

            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                if (!ParameterTypes[i].IsAssignableFrom(funcType.ParameterTypes[i]))
                    return false;
            }

            return true;
        }
        return false;
    }
}

/// <summary>
/// Promise type for async operations
/// </summary>
public class PromiseType : CxType
{
    public CxType ValueType { get; }
    public override string Name => $"Promise<{ValueType.Name}>";

    public PromiseType(CxType valueType)
    {
        ValueType = valueType;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        if (other is PromiseType promiseType)
        {
            return ValueType.IsAssignableFrom(promiseType.ValueType);
        }
        return false;
    }
}

/// <summary>
/// Object type with properties (for AI configurations)
/// </summary>
public class ObjectType : CxType
{
    public Dictionary<string, CxType> Properties { get; } = new();
    public override string Name => "object";

    public void AddProperty(string name, CxType type)
    {
        Properties[name] = type;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        if (other is ObjectType objType)
        {
            // Structural typing - check if all required properties are present
            foreach (var (propName, propType) in Properties)
            {
                if (!objType.Properties.TryGetValue(propName, out var otherPropType) ||
                    !propType.IsAssignableFrom(otherPropType))
                {
                    return false;
                }
            }
            return true;
        }
        return Name == "object" && other.Name == "object";
    }
}

/// <summary>
/// Custom user-defined type
/// </summary>
public class CustomType : CxType
{
    public override string Name { get; }
    public Dictionary<string, CxType> Members { get; } = new();

    public CustomType(string name)
    {
        Name = name;
    }

    public override bool IsAssignableFrom(CxType other)
    {
        return Name == other.Name;
    }
}
