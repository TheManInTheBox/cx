using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Enhanced printing utility for CX language that properly displays AI function results
    /// </summary>
    public static class CxPrint
    {
        /// <summary>
        /// Enhanced print function that properly displays Dictionary objects from AI functions
        /// </summary>
        public static void Print(object value)
        {
            if (value == null)
            {
                Console.WriteLine("null");
                return;
            }

            // Handle Dictionary objects from AI functions
            if (value is Dictionary<string, object> dict)
            {
                PrintDictionary(dict);
                return;
            }

            // Handle arrays
            if (value is Array array)
            {
                PrintArray(array);
                return;
            }

            // Handle other collection types
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                PrintEnumerable(enumerable);
                return;
            }

            // Handle primitive types (string, numbers, bool) - print as-is
            if (IsPrimitiveType(value))
            {
                Console.WriteLine(value);
                return;
            }

            // For complex objects, try CX-specific serialization first, then standard JSON
            try
            {
                // Check if this is a CX object (inherits from AiServiceBase)
                if (IsCxObject(value))
                {
                    PrintCxObject(value);
                    return;
                }
                
                // For other complex objects, serialize to JSON for better debugging
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                
                var json = JsonSerializer.Serialize(value, jsonOptions);
                Console.WriteLine(json);
            }
            catch (Exception ex)
            {
                // Fallback to ToString() if JSON serialization fails
                Console.WriteLine($"[Object: {value.GetType().Name}] {value} (JSON serialization failed: {ex.Message})");
            }
        }

        /// <summary>
        /// Check if a type should be printed as-is without JSON serialization
        /// </summary>
        private static bool IsPrimitiveType(object value)
        {
            var type = value.GetType();
            return type.IsPrimitive || 
                   type == typeof(string) || 
                   type == typeof(decimal) || 
                   type == typeof(DateTime) || 
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        /// <summary>
        /// Print a dictionary with proper formatting
        /// </summary>
        private static void PrintDictionary(Dictionary<string, object> dict)
        {
            Console.WriteLine("{");
            
            foreach (var kvp in dict)
            {
                Console.Write($"  {kvp.Key}: ");
                
                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    Console.WriteLine();
                    PrintNestedDictionary(nestedDict, "    ");
                }
                else if (kvp.Value is Array array)
                {
                    Console.WriteLine();
                    PrintNestedArray(array, "    ");
                }
                else if (kvp.Value is string str)
                {
                    // Handle multiline strings
                    if (str.Contains('\n'))
                    {
                        Console.WriteLine();
                        var lines = str.Split('\n');
                        foreach (var line in lines)
                        {
                            Console.WriteLine($"    {line}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\"{str}\"");
                    }
                }
                else
                {
                    Console.WriteLine(kvp.Value ?? "null");
                }
            }
            
            Console.WriteLine("}");
        }

        /// <summary>
        /// Print a nested dictionary with indentation
        /// </summary>
        private static void PrintNestedDictionary(Dictionary<string, object> dict, string indent)
        {
            Console.WriteLine($"{indent}{{");
            
            foreach (var kvp in dict)
            {
                Console.Write($"{indent}  {kvp.Key}: ");
                
                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    Console.WriteLine();
                    PrintNestedDictionary(nestedDict, indent + "    ");
                }
                else if (kvp.Value is string str)
                {
                    Console.WriteLine($"\"{str}\"");
                }
                else
                {
                    Console.WriteLine(kvp.Value ?? "null");
                }
            }
            
            Console.WriteLine($"{indent}}}");
        }

        /// <summary>
        /// Print an array with proper formatting
        /// </summary>
        private static void PrintArray(Array array)
        {
            Console.WriteLine("[");
            
            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                
                if (item is Dictionary<string, object> dict)
                {
                    Console.WriteLine("  {");
                    foreach (var kvp in dict)
                    {
                        Console.Write($"    {kvp.Key}: ");
                        
                        if (kvp.Value is Dictionary<string, object> nestedDict)
                        {
                            Console.WriteLine();
                            PrintNestedDictionary(nestedDict, "      ");
                        }
                        else if (kvp.Value is string str)
                        {
                            Console.WriteLine($"\"{str}\"");
                        }
                        else
                        {
                            Console.WriteLine(kvp.Value ?? "null");
                        }
                    }
                    Console.WriteLine("  }");
                }
                else if (item is string str)
                {
                    Console.WriteLine($"  \"{str}\"");
                }
                else
                {
                    Console.WriteLine($"  {item ?? "null"}");
                }
            }
            
            Console.WriteLine("]");
        }

        /// <summary>
        /// Print a nested array with indentation
        /// </summary>
        private static void PrintNestedArray(Array array, string indent)
        {
            Console.WriteLine($"{indent}[");
            
            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                
                if (item is Dictionary<string, object> nestedDict)
                {
                    Console.WriteLine($"{indent}  {{");
                    foreach (var kvp in nestedDict)
                    {
                        Console.Write($"{indent}    {kvp.Key}: ");
                        
                        if (kvp.Value is Dictionary<string, object> deepNestedDict)
                        {
                            Console.WriteLine();
                            PrintNestedDictionary(deepNestedDict, indent + "      ");
                        }
                        else if (kvp.Value is string str)
                        {
                            Console.WriteLine($"\"{str}\"");
                        }
                        else
                        {
                            Console.WriteLine(kvp.Value ?? "null");
                        }
                    }
                    Console.WriteLine($"{indent}  }}");
                }
                else if (item is string str)
                {
                    Console.WriteLine($"{indent}  \"{str}\"");
                }
                else
                {
                    Console.WriteLine($"{indent}  {item ?? "null"}");
                }
            }
            
            Console.WriteLine($"{indent}]");
        }

        /// <summary>
        /// Print an enumerable collection
        /// </summary>
        private static void PrintEnumerable(System.Collections.IEnumerable enumerable)
        {
            Console.WriteLine("[");
            
            foreach (var item in enumerable)
            {
                if (item is Dictionary<string, object> dict)
                {
                    Console.WriteLine("  {");
                    foreach (var kvp in dict)
                    {
                        Console.Write($"    {kvp.Key}: ");
                        
                        if (kvp.Value is Dictionary<string, object> nestedDict)
                        {
                            Console.WriteLine();
                            PrintNestedDictionary(nestedDict, "      ");
                        }
                        else if (kvp.Value is string str)
                        {
                            Console.WriteLine($"\"{str}\"");
                        }
                        else
                        {
                            Console.WriteLine(kvp.Value ?? "null");
                        }
                    }
                    Console.WriteLine("  }");
                }
                else if (item is string str)
                {
                    Console.WriteLine($"  \"{str}\"");
                }
                else
                {
                    Console.WriteLine($"  {item ?? "null"}");
                }
            }
            
            Console.WriteLine("]");
        }
        
        /// <summary>
        /// Check if an object is a CX object (inherits from AiServiceBase)
        /// </summary>
        private static bool IsCxObject(object value)
        {
            if (value == null) return false;
            
            var type = value.GetType();
            
            // Check if it inherits from AiServiceBase (the base class for all CX objects)
            while (type != null)
            {
                if (type.Name == "AiServiceBase")
                {
                    return true;
                }
                type = type.BaseType;
            }
            
            return false;
        }
        
        /// <summary>
        /// Print a CX object by extracting all its fields using reflection
        /// </summary>
        private static void PrintCxObject(object cxObject)
        {
            var objectType = cxObject.GetType();
            var fields = objectType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Console.WriteLine("{");
            
            bool hasFields = false;
            foreach (var field in fields)
            {
                // Skip internal fields like ServiceProvider, Logger, etc.
                if (field.Name.StartsWith("_") || 
                    field.Name == "ServiceProvider" || 
                    field.Name == "Logger" ||
                    field.Name.Contains("BackingField"))
                {
                    continue;
                }
                
                try
                {
                    var fieldValue = field.GetValue(cxObject);
                    Console.Write($"  \"{field.Name}\": ");
                    
                    if (fieldValue == null)
                    {
                        Console.WriteLine("null,");
                    }
                    else if (fieldValue is string str)
                    {
                        Console.WriteLine($"\"{str}\",");
                    }
                    else if (IsPrimitiveType(fieldValue))
                    {
                        Console.WriteLine($"{fieldValue},");
                    }
                    else if (fieldValue is Dictionary<string, object> dict)
                    {
                        Console.WriteLine();
                        PrintNestedDictionary(dict, "    ");
                        Console.WriteLine(",");
                    }
                    else
                    {
                        // For nested objects, check if it's another CX object first
                        if (IsCxObject(fieldValue))
                        {
                            Console.WriteLine();
                            PrintNestedCxObject(fieldValue, "    ");
                            Console.WriteLine(",");
                        }
                        else
                        {
                            // For other nested objects, try to serialize them
                            try
                            {
                                var json = JsonSerializer.Serialize(fieldValue, new JsonSerializerOptions { WriteIndented = false });
                                Console.WriteLine($"{json},");
                            }
                            catch
                            {
                                Console.WriteLine($"\"[Object: {fieldValue.GetType().Name}]\",");
                            }
                        }
                    }
                    
                    hasFields = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"[Error accessing field: {ex.Message}]\",");
                    hasFields = true;
                }
            }
            
            if (!hasFields)
            {
                Console.WriteLine("  \"[No accessible fields]\"");
            }
            
            Console.WriteLine("}");
        }
        
        /// <summary>
        /// Print a nested CX object with indentation
        /// </summary>
        private static void PrintNestedCxObject(object cxObject, string indent)
        {
            var objectType = cxObject.GetType();
            var fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            Console.WriteLine($"{indent}{{");
            
            bool hasFields = false;
            foreach (var field in fields)
            {
                // Skip internal fields like ServiceProvider, Logger, etc.
                if (field.Name.StartsWith("_") || 
                    field.Name == "ServiceProvider" || 
                    field.Name == "Logger" ||
                    field.Name.Contains("BackingField"))
                {
                    continue;
                }
                
                try
                {
                    var fieldValue = field.GetValue(cxObject);
                    Console.Write($"{indent}  \"{field.Name}\": ");
                    
                    if (fieldValue == null)
                    {
                        Console.WriteLine("null,");
                    }
                    else if (fieldValue is string str)
                    {
                        Console.WriteLine($"\"{str}\",");
                    }
                    else if (IsPrimitiveType(fieldValue))
                    {
                        Console.WriteLine($"{fieldValue},");
                    }
                    else if (fieldValue is Dictionary<string, object> dict)
                    {
                        Console.WriteLine();
                        PrintNestedDictionary(dict, indent + "    ");
                        Console.WriteLine(",");
                    }
                    else if (IsCxObject(fieldValue))
                    {
                        Console.WriteLine();
                        PrintNestedCxObject(fieldValue, indent + "    ");
                        Console.WriteLine(",");
                    }
                    else
                    {
                        // For other nested objects, try to serialize them
                        try
                        {
                            var json = JsonSerializer.Serialize(fieldValue, new JsonSerializerOptions { WriteIndented = false });
                            Console.WriteLine($"{json},");
                        }
                        catch
                        {
                            Console.WriteLine($"\"[Object: {fieldValue.GetType().Name}]\",");
                        }
                    }
                    
                    hasFields = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"[Error accessing field: {ex.Message}]\",");
                    hasFields = true;
                }
            }
            
            if (!hasFields)
            {
                Console.WriteLine($"{indent}  \"[No accessible fields]\"");
            }
            
            Console.WriteLine($"{indent}}}");
        }
    }
}
