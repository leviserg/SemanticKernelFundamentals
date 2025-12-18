using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace _04_Planners_Function_Calling.Plugins
{
    public class DatePlugin
    {
        [KernelFunction, Description("Gets the current date in default system format.")]
        public static string GetCurrentDate(IFormatProvider formatProvider = null!)
        {
            return DateTime.UtcNow.ToString("D", formatProvider);
        }
    }
}
