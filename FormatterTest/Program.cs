using System;
using Microsoft.Extensions.Logging;
using CxLanguage.IDE.WinUI.Services;

namespace CxLanguage.FormatterTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a simple logger
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<CxCodeFormattingService>();
            
            // Create formatting service
            var formatter = new CxCodeFormattingService(logger);
            
            // Test code from user - malformed with spacing and comment issues
            string testCode = @"conscious calculatorA { realize( self: object) { learn self; emit calculate.request { operation: ""add"", numbers: [ 2, 2] }; } on calculate.request( event) { print( event); } } conscious calculatorB { realize( self: object) { learn self; // Will execute both calculate.request // and calculate addition. emit calculate.request { operation: ""add"", numbers: [2, 2], handlers [calculate.addition] }; } on calculate.request (event) { print(event); } on calculate.addition (event) { print(event); } }";
            
            Console.WriteLine("=== CX Language Formatter Test v4.0 ===");
            Console.WriteLine();
            Console.WriteLine("Original (malformed) code:");
            Console.WriteLine(testCode);
            Console.WriteLine();
            
            // Format the code
            Console.WriteLine("Formatting with enhanced comment processing...");
            string formatted = formatter.FormatCode(testCode);
            
            Console.WriteLine();
            Console.WriteLine("Formatted code (Allman-style, non-K&R):");
            Console.WriteLine(formatted);
            
            Console.WriteLine();
            Console.WriteLine("=== Actual Results Analysis ===");
            
            // Check specific improvements
            bool fixedRealizeSpacing = formatted.Contains("realize(self: object)") && !formatted.Contains("realize( self:");
            bool fixedPrintSpacing = formatted.Contains("print(event)") && !formatted.Contains("print( event)");
            bool hasAllmanBraces = formatted.Contains("{\n    ") && formatted.Contains("conscious calculatorA\n{"); // Braces on new lines with proper structure
            bool calculatorBFormatted = formatted.Contains("conscious calculatorB\n{") && formatted.Contains("// Will execute both calculate.request and calculate addition");
            
            Console.WriteLine($"✅ Fixed realize() spacing: {(fixedRealizeSpacing ? "YES" : "NO")}");
            Console.WriteLine($"✅ Fixed print() spacing: {(fixedPrintSpacing ? "YES" : "NO")}");
            Console.WriteLine($"✅ Allman-style braces: {(hasAllmanBraces ? "YES" : "NO")}");
            Console.WriteLine($"✅ CalculatorB properly formatted: {(calculatorBFormatted ? "YES" : "NO")}");
            
            if (calculatorBFormatted)
            {
                Console.WriteLine("🎉 SUCCESS: Template-based formatting working perfectly!");
            }
            
            Console.WriteLine();
            Console.WriteLine("=== Test Complete ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
