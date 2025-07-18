using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.Serialization;

/// <summary>
/// JSON deserializer for converting JSON responses to native CX objects
/// Converts JSON objects to Dictionary&lt;string, object&gt; and JSON arrays to object[]
/// </summary>
public class CxJsonDeserializer
{
    private readonly ILogger<CxJsonDeserializer>? _logger;

    public CxJsonDeserializer(ILogger<CxJsonDeserializer>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Deserialize JSON string to native CX object
    /// </summary>
    /// <param name="jsonString">JSON string to deserialize</param>
    /// <returns>CX object (Dictionary&lt;string, object&gt; or object[] or primitive)</returns>
    public object? DeserializeToObject(string jsonString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }

            var jsonDocument = JsonDocument.Parse(jsonString);
            return ConvertJsonElement(jsonDocument.RootElement);
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning("Failed to parse JSON, returning original string: {Error}", ex.Message);
            // If JSON parsing fails, return the original string
            return jsonString;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deserializing JSON to CX object");
            return jsonString;
        }
    }

    /// <summary>
    /// Try to extract and deserialize JSON from a mixed text response
    /// Returns the deserialized object if JSON is found, otherwise returns the original string
    /// </summary>
    /// <param name="response">Response text that may contain JSON</param>
    /// <returns>CX object if JSON found, otherwise original string</returns>
    public object? TryDeserializeFromResponse(string response)
    {
        try
        {
            // Try to find JSON in the response
            var jsonString = ExtractJsonFromResponse(response);
            if (!string.IsNullOrEmpty(jsonString))
            {
                return DeserializeToObject(jsonString);
            }

            // If no JSON found, return original response
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error extracting JSON from response");
            return response;
        }
    }

    /// <summary>
    /// Convert JsonElement to native CX object
    /// </summary>
    private object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    /// <summary>
    /// Convert JSON object to Dictionary&lt;string, object&gt; (CX object)
    /// </summary>
    private Dictionary<string, object> ConvertJsonObject(JsonElement jsonObject)
    {
        var cxObject = new Dictionary<string, object>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var value = ConvertJsonElement(property.Value);
            if (value != null)
            {
                cxObject[property.Name] = value;
            }
        }

        return cxObject;
    }

    /// <summary>
    /// Convert JSON array to object[] (CX array)
    /// </summary>
    private object[] ConvertJsonArray(JsonElement jsonArray)
    {
        var cxArray = new List<object>();

        foreach (var element in jsonArray.EnumerateArray())
        {
            var value = ConvertJsonElement(element);
            if (value != null)
            {
                cxArray.Add(value);
            }
        }

        return cxArray.ToArray();
    }

    /// <summary>
    /// Extract JSON from mixed text response
    /// Looks for JSON objects or arrays within the response text
    /// </summary>
    private string? ExtractJsonFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        // Try to find JSON object
        var objectStart = response.IndexOf('{');
        if (objectStart >= 0)
        {
            var objectEnd = FindMatchingBrace(response, objectStart);
            if (objectEnd > objectStart)
            {
                return response.Substring(objectStart, objectEnd - objectStart + 1);
            }
        }

        // Try to find JSON array
        var arrayStart = response.IndexOf('[');
        if (arrayStart >= 0)
        {
            var arrayEnd = FindMatchingBracket(response, arrayStart);
            if (arrayEnd > arrayStart)
            {
                return response.Substring(arrayStart, arrayEnd - arrayStart + 1);
            }
        }

        return null;
    }

    /// <summary>
    /// Find matching closing brace for JSON object
    /// </summary>
    private int FindMatchingBrace(string text, int startIndex)
    {
        var braceCount = 0;
        var inString = false;
        var escaped = false;

        for (int i = startIndex; i < text.Length; i++)
        {
            var c = text[i];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (c == '{')
            {
                braceCount++;
            }
            else if (c == '}')
            {
                braceCount--;
                if (braceCount == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Find matching closing bracket for JSON array
    /// </summary>
    private int FindMatchingBracket(string text, int startIndex)
    {
        var bracketCount = 0;
        var inString = false;
        var escaped = false;

        for (int i = startIndex; i < text.Length; i++)
        {
            var c = text[i];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (c == '[')
            {
                bracketCount++;
            }
            else if (c == ']')
            {
                bracketCount--;
                if (bracketCount == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
