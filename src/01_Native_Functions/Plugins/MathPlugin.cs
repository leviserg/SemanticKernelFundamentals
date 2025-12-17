using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace _01_Native_Functions.Plugins
{
    public class MathPlugin
    {
        [KernelFunction("math_sqrt")]
        [Description("Calculates the square root of a given number.")]
        public static double Sqrt(
            [Description("The number to calculate the square root of.")]
            double value)
        {
            return Math.Sqrt(value);
        }
    }
}
