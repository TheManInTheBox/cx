using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CxLanguage.Core.Telemetry
{
    /// <summary>
    /// Telemetry service for CX Language runtime metrics
    /// </summary>
    public class CxTelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<CxTelemetryService> _logger;

        public CxTelemetryService(TelemetryClient telemetryClient, ILogger<CxTelemetryService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        /// <summary>
        /// Track execution of a CX script
        /// </summary>
        public void TrackScriptExecution(string scriptName, TimeSpan duration, bool success, string? errorMessage = null)
        {
            var telemetry = new EventTelemetry("CX_ScriptExecution");
            telemetry.Properties["ScriptName"] = scriptName;
            telemetry.Properties["Success"] = success.ToString();
            telemetry.Metrics["Duration"] = duration.TotalMilliseconds;
            
            if (!success && errorMessage != null)
            {
                telemetry.Properties["ErrorMessage"] = errorMessage;
            }

            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation("Script execution tracked: {ScriptName}, Duration: {Duration}ms, Success: {Success}", 
                scriptName, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// Track AI function execution
        /// </summary>
        public void TrackAiFunctionExecution(string functionName, string goal, TimeSpan duration, bool success, string? errorMessage = null)
        {
            var telemetry = new EventTelemetry("CX_AIFunctionExecution");
            telemetry.Properties["FunctionName"] = functionName;
            telemetry.Properties["Goal"] = goal;
            telemetry.Properties["Success"] = success.ToString();
            telemetry.Metrics["Duration"] = duration.TotalMilliseconds;
            
            if (!success && errorMessage != null)
            {
                telemetry.Properties["ErrorMessage"] = errorMessage;
            }

            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation("AI function execution tracked: {FunctionName}, Duration: {Duration}ms, Success: {Success}", 
                functionName, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// Track compilation metrics
        /// </summary>
        public void TrackCompilation(string scriptName, TimeSpan duration, bool success, int linesOfCode, string? errorMessage = null)
        {
            var telemetry = new EventTelemetry("CX_Compilation");
            telemetry.Properties["ScriptName"] = scriptName;
            telemetry.Properties["Success"] = success.ToString();
            telemetry.Metrics["Duration"] = duration.TotalMilliseconds;
            telemetry.Metrics["LinesOfCode"] = linesOfCode;
            
            if (!success && errorMessage != null)
            {
                telemetry.Properties["ErrorMessage"] = errorMessage;
            }

            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation("Compilation tracked: {ScriptName}, Duration: {Duration}ms, LOC: {LinesOfCode}, Success: {Success}", 
                scriptName, duration.TotalMilliseconds, linesOfCode, success);
        }

        /// <summary>
        /// Track Azure OpenAI API usage
        /// </summary>
        public void TrackAzureOpenAIUsage(string operation, TimeSpan duration, bool success, int? tokenCount = null, string? errorMessage = null)
        {
            var telemetry = new EventTelemetry("CX_AzureOpenAIUsage");
            telemetry.Properties["Operation"] = operation;
            telemetry.Properties["Success"] = success.ToString();
            telemetry.Metrics["Duration"] = duration.TotalMilliseconds;
            
            if (tokenCount.HasValue)
            {
                telemetry.Metrics["TokenCount"] = tokenCount.Value;
            }
            
            if (!success && errorMessage != null)
            {
                telemetry.Properties["ErrorMessage"] = errorMessage;
            }

            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation("Azure OpenAI usage tracked: {Operation}, Duration: {Duration}ms, Success: {Success}", 
                operation, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// Track custom performance metrics
        /// </summary>
        public void TrackPerformance(string operationName, TimeSpan duration, Dictionary<string, string>? properties = null)
        {
            var telemetry = new EventTelemetry("CX_Performance");
            telemetry.Properties["OperationName"] = operationName;
            telemetry.Metrics["Duration"] = duration.TotalMilliseconds;
            
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    telemetry.Properties[kvp.Key] = kvp.Value;
                }
            }

            _telemetryClient.TrackEvent(telemetry);
        }

        /// <summary>
        /// Track exceptions
        /// </summary>
        public void TrackException(Exception exception, string operationName, Dictionary<string, string>? properties = null)
        {
            var telemetry = new ExceptionTelemetry(exception);
            telemetry.Properties["OperationName"] = operationName;
            
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    telemetry.Properties[kvp.Key] = kvp.Value;
                }
            }

            _telemetryClient.TrackException(telemetry);
            _logger.LogError(exception, "Exception tracked for operation: {OperationName}", operationName);
        }

        /// <summary>
        /// Flush all telemetry data
        /// </summary>
        public void Flush()
        {
            _telemetryClient.Flush();
        }
    }
}
