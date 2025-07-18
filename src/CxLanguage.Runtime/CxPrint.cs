using System;
using System.Collections.Generic;
using System.Linq;
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

            // Default behavior for other types
            Console.WriteLine(value);
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
            Console.Write("[");
            
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0) Console.Write(", ");
                
                var item = array.GetValue(i);
                if (item is string str)
                {
                    Console.Write($"\"{str}\"");
                }
                else
                {
                    Console.Write(item ?? "null");
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
                Console.Write($"{indent}  ");
                
                var item = array.GetValue(i);
                if (item is string str)
                {
                    Console.WriteLine($"\"{str}\"");
                }
                else
                {
                    Console.WriteLine(item ?? "null");
                }
            }
            
            Console.WriteLine($"{indent}]");
        }

        /// <summary>
        /// Print an enumerable collection
        /// </summary>
        private static void PrintEnumerable(System.Collections.IEnumerable enumerable)
        {
            Console.Write("[");
            
            bool first = true;
            foreach (var item in enumerable)
            {
                if (!first) Console.Write(", ");
                first = false;
                
                if (item is string str)
                {
                    Console.Write($"\"{str}\"");
                }
                else
                {
                    Console.Write(item ?? "null");
                }
            }
            
            Console.WriteLine("]");
        }
    }
}
