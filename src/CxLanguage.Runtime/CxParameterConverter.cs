using System;
using System.Collections.Generic;
using System.Reflection;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Helper class for converting CX runtime values to .NET types
    /// </summary>
    public static class CxParameterConverter
    {
        /// <summary>
        /// Convert a Dictionary<string, object> to a strongly-typed options object
        /// </summary>
        /// <param name="dictionary">Source dictionary from CX object literal</param>
        /// <param name="targetType">Target .NET type to convert to</param>
        /// <returns>Converted object instance</returns>
        public static object? ConvertToOptions(object? input, Type targetType)
        {
            if (input == null)
                return null;
                
            if (!(input is Dictionary<string, object> dictionary))
            {
                // If input is not a dictionary, try to return as-is
                return input;
            }

            try
            {
                // Create instance of target type
                var instance = Activator.CreateInstance(targetType);
                if (instance == null)
                    return null;

                // Get all public properties of the target type
                var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (!property.CanWrite)
                        continue;

                    // Try to find matching key in dictionary (case-insensitive)
                    var matchingKey = FindMatchingKey(dictionary, property.Name);
                    if (matchingKey == null)
                        continue;

                    var value = dictionary[matchingKey];
                    if (value == null)
                        continue;

                    // Convert value to property type
                    var convertedValue = ConvertValue(value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(instance, convertedValue);
                    }
                }

                return instance;
            }
            catch (Exception ex)
            {
                // Log error and return null
                Console.WriteLine($"[ERROR] Failed to convert dictionary to {targetType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find a matching key in the dictionary (case-insensitive)
        /// </summary>
        private static string? FindMatchingKey(Dictionary<string, object> dictionary, string targetKey)
        {
            // Try exact match first
            if (dictionary.ContainsKey(targetKey))
                return targetKey;

            // Try case-insensitive match
            foreach (var key in dictionary.Keys)
            {
                if (string.Equals(key, targetKey, StringComparison.OrdinalIgnoreCase))
                    return key;
            }

            return null;
        }

        /// <summary>
        /// Convert a value to the target type
        /// </summary>
        private static object? ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            // Handle nullable types
            var actualTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If value is already the correct type, return as-is
            if (actualTargetType.IsInstanceOfType(value))
                return value;

            try
            {
                // Handle string conversions
                if (actualTargetType == typeof(string))
                    return value.ToString();

                // Handle numeric conversions
                if (actualTargetType == typeof(int))
                    return Convert.ToInt32(value);
                    
                if (actualTargetType == typeof(double))
                    return Convert.ToDouble(value);
                    
                if (actualTargetType == typeof(float))
                    return Convert.ToSingle(value);
                    
                if (actualTargetType == typeof(bool))
                    return Convert.ToBoolean(value);

                // Handle array conversions
                if (actualTargetType.IsArray && value is object[] array)
                {
                    var elementType = actualTargetType.GetElementType()!;
                    var convertedArray = Array.CreateInstance(elementType, array.Length);
                    
                    for (int i = 0; i < array.Length; i++)
                    {
                        var convertedElement = ConvertValue(array[i], elementType);
                        convertedArray.SetValue(convertedElement, i);
                    }
                    
                    return convertedArray;
                }

                // Try generic conversion
                return Convert.ChangeType(value, actualTargetType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to convert {value.GetType().Name} to {actualTargetType.Name}: {ex.Message}");
                return null;
            }
        }
    }
}

