using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.Services
{
    /// <summary>
    /// Handles system.time.* events for date/time operations with consciousness-aware patterns
    /// Supports current time retrieval, parsing, formatting, arithmetic, and timezone operations
    /// </summary>
    public class TimeService
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<TimeService> _logger;

        // Predefined format strings for common date/time formats
        private static readonly Dictionary<string, string> PredefinedFormats = new()
        {
            { "ISO8601", "yyyy-MM-ddTHH:mm:ssZ" },
            { "iso8601", "yyyy-MM-ddTHH:mm:ssZ" },
            { "ISO", "yyyy-MM-ddTHH:mm:ssZ" },
            { "iso", "yyyy-MM-ddTHH:mm:ssZ" },
            { "datetime", "yyyy-MM-dd HH:mm:ss" },
            { "date", "yyyy-MM-dd" },
            { "time", "HH:mm:ss" },
            { "timestamp", "yyyy-MM-ddTHH:mm:ss.fffZ" },
            { "short", "M/d/yyyy" },
            { "long", "MMMM d, yyyy" },
            { "full", "dddd, MMMM d, yyyy HH:mm:ss" }
        };

        public TimeService(ICxEventBus eventBus, ILogger<TimeService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to time operation events
            _eventBus.Subscribe("system.time.now", HandleTimeNowAsync);
            _eventBus.Subscribe("system.time.parse", HandleTimeParseAsync);
            _eventBus.Subscribe("system.time.format", HandleTimeFormatAsync);
            _eventBus.Subscribe("system.time.add", HandleTimeAddAsync);
            _eventBus.Subscribe("system.time.diff", HandleTimeDiffAsync);
            _eventBus.Subscribe("system.time.timezone", HandleTimeZoneAsync);
            
            _logger.LogInformation("TimeService subscribed to time events with consciousness-aware patterns");
        }

        /// <summary>
        /// Handler for 'system.time.now' event
        /// Supports payloads: { format: string?, timezone: string?, handlers: array? }
        /// Returns current date/time in specified format and timezone
        /// </summary>
        private async Task<bool> HandleTimeNowAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Get current time
                var now = DateTimeOffset.UtcNow;
                
                // Handle timezone conversion
                if (payload.TryGetValue("timezone", out var timezoneObj) && timezoneObj is string timezoneId)
                {
                    try
                    {
                        if (timezoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                        {
                            now = DateTimeOffset.UtcNow;
                        }
                        else if (timezoneId.Equals("local", StringComparison.OrdinalIgnoreCase))
                        {
                            now = DateTimeOffset.Now;
                        }
                        else
                        {
                            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                            now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
                        }
                        _logger.LogDebug("Converted to timezone: {Timezone}", timezoneId);
                    }
                    catch (TimeZoneNotFoundException ex)
                    {
                        _logger.LogWarning(ex, "Invalid timezone '{Timezone}', using UTC", timezoneId);
                        now = DateTimeOffset.UtcNow;
                    }
                }

                // Format the time
                var formatString = GetFormatString(payload);
                var formattedTime = FormatDateTime(now, formatString);
                
                // Prepare result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "timestamp", formattedTime },
                    { "format", formatString },
                    { "timezone", now.Offset.ToString() },
                    { "unixTimestamp", now.ToUnixTimeSeconds() },
                    { "iso8601", now.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "utc", now.UtcDateTime }
                };

                // Emit success event
                await _eventBus.EmitAsync("system.time.now.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.now event");
                await EmitErrorEvent("system.time.now.error", $"Time retrieval failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.time.parse' event
        /// Supports payloads: { value: string, format: string?, timezone: string?, handlers: array? }
        /// Parses string to datetime object
        /// </summary>
        private async Task<bool> HandleTimeParseAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract value to parse
                if (!payload.TryGetValue("value", out var valueObj) || valueObj is not string dateString || string.IsNullOrWhiteSpace(dateString))
                {
                    _logger.LogError("system.time.parse requires 'value' parameter");
                    await EmitErrorEvent("system.time.parse.error", "Missing or invalid 'value' parameter", payload);
                    return false;
                }

                var formatString = GetFormatString(payload);
                DateTimeOffset parsedTime;

                try
                {
                    // Try parsing with specified format
                    if (PredefinedFormats.ContainsKey(formatString) || formatString.Contains("yyyy") || formatString.Contains("MM"))
                    {
                        var actualFormat = PredefinedFormats.GetValueOrDefault(formatString, formatString);
                        parsedTime = DateTimeOffset.ParseExact(dateString, actualFormat, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Try general parsing
                        parsedTime = DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture);
                    }

                    _logger.LogDebug("Parsed date string '{DateString}' with format '{Format}'", dateString, formatString);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Failed to parse date string '{DateString}' with format '{Format}'", dateString, formatString);
                    await EmitErrorEvent("system.time.parse.error", $"Date parsing failed: {ex.Message}", payload);
                    return false;
                }

                // Prepare result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "timestamp", parsedTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "originalValue", dateString },
                    { "format", formatString },
                    { "unixTimestamp", parsedTime.ToUnixTimeSeconds() },
                    { "iso8601", parsedTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "dateTime", parsedTime.DateTime }
                };

                // Emit success event
                await _eventBus.EmitAsync("system.time.parse.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.parse event");
                await EmitErrorEvent("system.time.parse.error", $"Time parsing failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.time.format' event
        /// Supports payloads: { timestamp: long|string, format: string?, timezone: string?, handlers: array? }
        /// Formats timestamp to string
        /// </summary>
        private async Task<bool> HandleTimeFormatAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract timestamp
                if (!payload.TryGetValue("timestamp", out var timestampObj))
                {
                    _logger.LogError("system.time.format requires 'timestamp' parameter");
                    await EmitErrorEvent("system.time.format.error", "Missing 'timestamp' parameter", payload);
                    return false;
                }

                DateTimeOffset dateTime;

                // Convert timestamp to DateTimeOffset
                try
                {
                    if (timestampObj is long unixTimestamp)
                    {
                        dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    }
                    else if (timestampObj is string timestampString)
                    {
                        dateTime = DateTimeOffset.Parse(timestampString, CultureInfo.InvariantCulture);
                    }
                    else if (timestampObj is DateTime dt)
                    {
                        dateTime = new DateTimeOffset(dt);
                    }
                    else
                    {
                        await EmitErrorEvent("system.time.format.error", "Invalid timestamp format", payload);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert timestamp");
                    await EmitErrorEvent("system.time.format.error", $"Timestamp conversion failed: {ex.Message}", payload);
                    return false;
                }

                var formatString = GetFormatString(payload);
                var formattedTime = FormatDateTime(dateTime, formatString);

                // Prepare result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "formatted", formattedTime },
                    { "originalTimestamp", timestampObj },
                    { "format", formatString },
                    { "iso8601", dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "unixTimestamp", dateTime.ToUnixTimeSeconds() }
                };

                // Emit success event
                await _eventBus.EmitAsync("system.time.format.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.format event");
                await EmitErrorEvent("system.time.format.error", $"Time formatting failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.time.add' event
        /// Supports payloads: { timestamp: long|string, years: int?, months: int?, days: int?, hours: int?, minutes: int?, seconds: int?, handlers: array? }
        /// Adds time intervals to dates
        /// </summary>
        private async Task<bool> HandleTimeAddAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract timestamp
                if (!payload.TryGetValue("timestamp", out var timestampObj))
                {
                    _logger.LogError("system.time.add requires 'timestamp' parameter");
                    await EmitErrorEvent("system.time.add.error", "Missing 'timestamp' parameter", payload);
                    return false;
                }

                DateTimeOffset dateTime;

                // Convert timestamp to DateTimeOffset
                try
                {
                    if (timestampObj is long unixTimestamp)
                    {
                        dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    }
                    else if (timestampObj is string timestampString)
                    {
                        dateTime = DateTimeOffset.Parse(timestampString, CultureInfo.InvariantCulture);
                    }
                    else if (timestampObj is DateTime dt)
                    {
                        dateTime = new DateTimeOffset(dt);
                    }
                    else
                    {
                        await EmitErrorEvent("system.time.add.error", "Invalid timestamp format", payload);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert timestamp");
                    await EmitErrorEvent("system.time.add.error", $"Timestamp conversion failed: {ex.Message}", payload);
                    return false;
                }

                // Add time intervals
                var originalDateTime = dateTime;
                
                if (payload.TryGetValue("years", out var yearsObj) && int.TryParse(yearsObj?.ToString(), out var years))
                    dateTime = dateTime.AddYears(years);
                
                if (payload.TryGetValue("months", out var monthsObj) && int.TryParse(monthsObj?.ToString(), out var months))
                    dateTime = dateTime.AddMonths(months);
                
                if (payload.TryGetValue("days", out var daysObj) && int.TryParse(daysObj?.ToString(), out var days))
                    dateTime = dateTime.AddDays(days);
                
                if (payload.TryGetValue("hours", out var hoursObj) && int.TryParse(hoursObj?.ToString(), out var hours))
                    dateTime = dateTime.AddHours(hours);
                
                if (payload.TryGetValue("minutes", out var minutesObj) && int.TryParse(minutesObj?.ToString(), out var minutes))
                    dateTime = dateTime.AddMinutes(minutes);
                
                if (payload.TryGetValue("seconds", out var secondsObj) && int.TryParse(secondsObj?.ToString(), out var seconds))
                    dateTime = dateTime.AddSeconds(seconds);

                // Prepare result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "timestamp", dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "originalTimestamp", originalDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "unixTimestamp", dateTime.ToUnixTimeSeconds() },
                    { "iso8601", dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "dateTime", dateTime.DateTime }
                };

                // Emit success event
                await _eventBus.EmitAsync("system.time.add.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.add event");
                await EmitErrorEvent("system.time.add.error", $"Time addition failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.time.diff' event
        /// Supports payloads: { start: long|string, end: long|string, unit: string?, handlers: array? }
        /// Calculates time differences
        /// </summary>
        private async Task<bool> HandleTimeDiffAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract start and end timestamps
                if (!payload.TryGetValue("start", out var startObj) || !payload.TryGetValue("end", out var endObj))
                {
                    _logger.LogError("system.time.diff requires 'start' and 'end' parameters");
                    await EmitErrorEvent("system.time.diff.error", "Missing 'start' or 'end' parameter", payload);
                    return false;
                }

                DateTimeOffset startTime, endTime;

                // Convert timestamps to DateTimeOffset
                try
                {
                    startTime = ConvertToDateTimeOffset(startObj);
                    endTime = ConvertToDateTimeOffset(endObj);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert timestamps");
                    await EmitErrorEvent("system.time.diff.error", $"Timestamp conversion failed: {ex.Message}", payload);
                    return false;
                }

                var timeSpan = endTime - startTime;
                var unit = payload.TryGetValue("unit", out var unitObj) ? unitObj?.ToString()?.ToLower() : "milliseconds";

                double difference = unit switch
                {
                    "years" => timeSpan.TotalDays / 365.25,
                    "months" => timeSpan.TotalDays / 30.44, // Average month length
                    "days" => timeSpan.TotalDays,
                    "hours" => timeSpan.TotalHours,
                    "minutes" => timeSpan.TotalMinutes,
                    "seconds" => timeSpan.TotalSeconds,
                    "milliseconds" => timeSpan.TotalMilliseconds,
                    _ => timeSpan.TotalMilliseconds
                };

                // Prepare result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "difference", difference },
                    { "unit", unit ?? "milliseconds" },
                    { "start", startTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "end", endTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "totalMilliseconds", timeSpan.TotalMilliseconds },
                    { "totalSeconds", timeSpan.TotalSeconds },
                    { "totalMinutes", timeSpan.TotalMinutes },
                    { "totalHours", timeSpan.TotalHours },
                    { "totalDays", timeSpan.TotalDays }
                };

                // Emit success event
                await _eventBus.EmitAsync("system.time.diff.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.diff event");
                await EmitErrorEvent("system.time.diff.error", $"Time difference calculation failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.time.timezone' event
        /// Supports payloads: { list: bool?, current: bool?, convert: { from: string, to: string, timestamp: string }?, handlers: array? }
        /// Gets timezone information
        /// </summary>
        private async Task<bool> HandleTimeZoneAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                var resultPayload = new Dictionary<string, object>();

                // List all available timezones
                if (payload.TryGetValue("list", out var listObj) && listObj is bool list && list)
                {
                    var timezones = TimeZoneInfo.GetSystemTimeZones()
                        .Select(tz => new { id = tz.Id, displayName = tz.DisplayName, standardName = tz.StandardName })
                        .ToArray();
                    resultPayload["timezones"] = timezones;
                }

                // Get current timezone info
                if (payload.TryGetValue("current", out var currentObj) && currentObj is bool current && current)
                {
                    var localTimeZone = TimeZoneInfo.Local;
                    resultPayload["currentTimezone"] = new
                    {
                        id = localTimeZone.Id,
                        displayName = localTimeZone.DisplayName,
                        standardName = localTimeZone.StandardName,
                        isDaylightSavingTime = localTimeZone.IsDaylightSavingTime(DateTime.Now),
                        utcOffset = localTimeZone.GetUtcOffset(DateTime.Now).ToString()
                    };
                }

                // Convert timezone
                if (payload.TryGetValue("convert", out var convertObj) && convertObj is IDictionary<string, object> convertParams)
                {
                    if (convertParams.TryGetValue("from", out var fromObj) && fromObj is string fromTz &&
                        convertParams.TryGetValue("to", out var toObj) && toObj is string toTz &&
                        convertParams.TryGetValue("timestamp", out var timestampObj) && timestampObj is string timestamp)
                    {
                        try
                        {
                            var fromTimeZone = TimeZoneInfo.FindSystemTimeZoneById(fromTz);
                            var toTimeZone = TimeZoneInfo.FindSystemTimeZoneById(toTz);
                            var dateTime = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
                            
                            var convertedTime = TimeZoneInfo.ConvertTime(dateTime, toTimeZone);
                            
                            resultPayload["converted"] = new
                            {
                                original = timestamp,
                                fromTimezone = fromTz,
                                toTimezone = toTz,
                                convertedTime = convertedTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
                            };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to convert timezone");
                            resultPayload["conversionError"] = ex.Message;
                        }
                    }
                }

                // Emit success event
                await _eventBus.EmitAsync("system.time.timezone.success", resultPayload);

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.time.timezone event");
                await EmitErrorEvent("system.time.timezone.error", $"Timezone operation failed: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Helper method to get format string from payload
        /// </summary>
        private static string GetFormatString(IDictionary<string, object>? payload)
        {
            if (payload?.TryGetValue("format", out var formatObj) == true && formatObj is string format && !string.IsNullOrEmpty(format))
            {
                return PredefinedFormats.GetValueOrDefault(format.ToLower(), format);
            }
            return "yyyy-MM-ddTHH:mm:ssZ"; // Default to ISO8601
        }

        /// <summary>
        /// Helper method to format DateTime with the given format string
        /// </summary>
        private static string FormatDateTime(DateTimeOffset dateTime, string formatString)
        {
            try
            {
                return dateTime.ToString(formatString, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                // Fallback to ISO8601 if format is invalid
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Helper method to convert various timestamp formats to DateTimeOffset
        /// </summary>
        private static DateTimeOffset ConvertToDateTimeOffset(object timestampObj)
        {
            return timestampObj switch
            {
                long unixTimestamp => DateTimeOffset.FromUnixTimeSeconds(unixTimestamp),
                string timestampString => DateTimeOffset.Parse(timestampString, CultureInfo.InvariantCulture),
                DateTime dt => new DateTimeOffset(dt),
                DateTimeOffset dto => dto,
                _ => throw new ArgumentException($"Unsupported timestamp type: {timestampObj?.GetType()}")
            };
        }

        /// <summary>
        /// Helper method to emit error events with standardized format
        /// </summary>
        private async Task EmitErrorEvent(string eventName, string errorMessage, IDictionary<string, object> originalPayload)
        {
            var errorPayload = new Dictionary<string, object>
            {
                { "error", errorMessage },
                { "timestamp", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "originalPayload", new Dictionary<string, object>(originalPayload) }
            };

            await _eventBus.EmitAsync(eventName, errorPayload);
        }
    }
}
