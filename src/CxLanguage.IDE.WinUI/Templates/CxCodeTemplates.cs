using System.Collections.Generic;

namespace CxLanguage.IDE.WinUI.Templates
{
    public static class CxCodeTemplates
    {
        public const string DualCalculatorTemplate = @"conscious calculatorA 
{
    realize(self: object) 
    {
        learn self;
        emit calculate.request 
        {
            operation: ""add"",
            numbers: [2, 2]
        };
    }
    
    on calculate.request(event) 
    {
        print(event);
    }
}

conscious calculatorB 
{
    realize(self: object) 
    {
        learn self;
        // Will execute both calculate.request and calculate addition
        emit calculate.request 
        {
            operation: ""add"",
            numbers: [2, 2],
            handlers: [calculate.addition]
        };
    }
    
    on calculate.request(event) 
    {
        print(event);
    }
    
    on calculate.addition(event) 
    {
        print(event);
    }
}";

        public const string SingleCalculatorTemplate = @"conscious calculator 
{
    realize(self: object) 
    {
        learn self;
        emit calculate.request 
        {
            operation: ""add"",
            numbers: [2, 2]
        };
    }
    
    on calculate.request(event) 
    {
        print(event);
    }
}";

        public const string BasicConsciousEntityTemplate = @"conscious {0} 
{{
    realize(self: object) 
    {{
        learn self;
        // TODO: Add initialization logic
    }}
    
    // TODO: Add event handlers
}}";

        // Template patterns for different CX constructs
        public static readonly Dictionary<string, string> Patterns = new Dictionary<string, string>
        {
            ["conscious_entity"] = @"conscious {name} 
{{
    realize(self: object) 
    {{
        learn self;
        {body}
    }}
    {handlers}
}}",
            
            ["event_handler"] = @"on {event}(event) 
{{
    {body}
}}",
            
            ["emit_statement"] = @"emit {event} 
{{
    {properties}
}};",
            
            ["think_statement"] = @"think 
{{
    data: ""{prompt}"",
    handlers: [{handlers}]
}};",
            
            ["learn_statement"] = @"learn 
{{
    data: ""{data}"",
    handlers: [{handlers}]
}};",
            
            ["adapt_statement"] = @"adapt 
{{
    context: ""{context}"",
    focus: ""{focus}"",
    data: {{
        currentCapabilities: ""{current}"",
        targetCapabilities: ""{target}"",
        learningObjective: ""{objective}""
    }},
    handlers: [{handlers}]
}};"
        };
    }
}
