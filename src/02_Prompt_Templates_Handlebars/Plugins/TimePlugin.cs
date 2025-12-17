using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace _02_Prompt_Templates_Handlebars.Plugins
{
    public class TimePlugin
    {

        [KernelFunction(nameof(GetCurrentTime))]
        [Description("Gets the current system time in default HH:mm:ss format.")]
        public static string GetCurrentTime(IFormatProvider formatProvider = null!)
        {
            return DateTime.Now.ToString("HH:mm:ss", formatProvider);
        }
    }
}
