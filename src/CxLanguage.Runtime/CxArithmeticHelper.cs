using System;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Helper class for runtime arithmetic operations with automatic type coercion
    /// </summary>
    public static class CxArithmeticHelper
    {
        /// <summary>
        /// Perform arithmetic operations with automatic type coercion between int and double
        /// </summary>
        public static object PerformArithmetic(object left, object right, string operation)
        {
            // Handle null cases
            if (left == null || right == null)
                throw new InvalidOperationException("Cannot perform arithmetic on null values");

            // Determine the actual runtime types
            var leftValue = ConvertToNumeric(left);
            var rightValue = ConvertToNumeric(right);

            // Promote to double if either operand is double
            if (leftValue is double || rightValue is double)
            {
                var leftDouble = Convert.ToDouble(leftValue);
                var rightDouble = Convert.ToDouble(rightValue);

                return operation.ToLowerInvariant() switch
                {
                    "add" => leftDouble + rightDouble,
                    "sub" => leftDouble - rightDouble,
                    "mul" => leftDouble * rightDouble,
                    "div" => leftDouble / rightDouble,
                    "rem" => leftDouble % rightDouble,
                    _ => throw new InvalidOperationException($"Unknown arithmetic operation: {operation}")
                };
            }
            else
            {
                // Both are integers
                var leftInt = Convert.ToInt32(leftValue);
                var rightInt = Convert.ToInt32(rightValue);

                return operation.ToLowerInvariant() switch
                {
                    "add" => leftInt + rightInt,
                    "sub" => leftInt - rightInt,
                    "mul" => leftInt * rightInt,
                    "div" => leftInt / rightInt,
                    "rem" => leftInt % rightInt,
                    _ => throw new InvalidOperationException($"Unknown arithmetic operation: {operation}")
                };
            }
        }

        /// <summary>
        /// Perform comparison operations with automatic type coercion between int and double
        /// </summary>
        public static object PerformComparison(object left, object right, string operation)
        {
            // Handle null cases
            if (left == null || right == null)
                throw new InvalidOperationException("Cannot perform comparison on null values");

            // Determine the actual runtime types and convert to numeric
            var leftValue = ConvertToNumeric(left);
            var rightValue = ConvertToNumeric(right);

            // Convert both to double for consistent comparison
            var leftDouble = Convert.ToDouble(leftValue);
            var rightDouble = Convert.ToDouble(rightValue);

            return operation.ToLowerInvariant() switch
            {
                "lt" => leftDouble < rightDouble,
                "le" => leftDouble <= rightDouble,
                "gt" => leftDouble > rightDouble,
                "ge" => leftDouble >= rightDouble,
                _ => throw new InvalidOperationException($"Unknown comparison operation: {operation}")
            };
        }

        /// <summary>
        /// Convert a boxed value to its underlying numeric type
        /// </summary>
        private static object ConvertToNumeric(object value)
        {
            return value switch
            {
                int i => i,
                double d => d,
                float f => (double)f,
                long l => (double)l,
                decimal dec => (double)dec,
                _ => throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to numeric type")
            };
        }
    }
}
