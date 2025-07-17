using System.Reflection;
using System.Reflection.Emit;

namespace CxLanguage.Compiler.Services;

/// <summary>
/// Helper for generating IL to inject runtime services into compiled assemblies
/// </summary>
public static class ServiceInjectionHelper
{
    /// <summary>
    /// Generate IL to create a service accessor field/property
    /// </summary>
    public static void EmitServiceAccessor(ILGenerator il, TypeBuilder typeBuilder, string serviceName, Type serviceType)
    {
        // Create private field for the service
        var serviceField = typeBuilder.DefineField($"_{serviceName}Service", serviceType, FieldAttributes.Private);
        
        // Create constructor parameter to inject the service
        var constructor = typeBuilder.DefineConstructor(
            MethodAttributes.Public, 
            CallingConventions.Standard, 
            new[] { serviceType });
        
        var constructorIl = constructor.GetILGenerator();
        
        // Call base constructor
        constructorIl.Emit(OpCodes.Ldarg_0);
        constructorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        
        // Store service parameter in field
        constructorIl.Emit(OpCodes.Ldarg_0);
        constructorIl.Emit(OpCodes.Ldarg_1);
        constructorIl.Emit(OpCodes.Stfld, serviceField);
        
        constructorIl.Emit(OpCodes.Ret);
        
        // Create property to access the service
        var property = typeBuilder.DefineProperty(serviceName, PropertyAttributes.None, serviceType, null);
        
        var getter = typeBuilder.DefineMethod($"get_{serviceName}", 
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            serviceType, Type.EmptyTypes);
            
        var getterIl = getter.GetILGenerator();
        getterIl.Emit(OpCodes.Ldarg_0);
        getterIl.Emit(OpCodes.Ldfld, serviceField);
        getterIl.Emit(OpCodes.Ret);
        
        property.SetGetMethod(getter);
    }
    
    /// <summary>
    /// Emit IL to load a service and call a method on it
    /// </summary>
    public static void EmitServiceMethodCall(ILGenerator il, FieldInfo serviceField, MethodInfo serviceMethod)
    {
        // Load the service instance
        il.Emit(OpCodes.Ldarg_0); // 'this'
        il.Emit(OpCodes.Ldfld, serviceField);
        
        // Arguments are already on the stack from the caller
        
        // Call the service method
        il.Emit(OpCodes.Callvirt, serviceMethod);
    }
}
