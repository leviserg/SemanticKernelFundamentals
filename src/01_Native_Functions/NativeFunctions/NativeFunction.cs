using _01_Native_Functions.Plugins;
using Microsoft.SemanticKernel;

namespace _01_Native_Functions.NativeFunctions
{
    public class NativeFunction
    {

        public static async Task Execute()
        {

            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
            var name = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: name,
                endpoint: endpoint,
                apiKey: apiKey);

            builder.Plugins.AddFromType<MathPlugin>();

            var kernel = builder.Build();

            var inputNumber = 81.0;

            var arguments = new KernelArguments
            {
                { "value", inputNumber }
            };

            var result = await kernel.InvokeAsync(nameof(MathPlugin), "math_sqrt", arguments);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"AI says: {result}");
            Console.ForegroundColor = ConsoleColor.White;

        }
    }
}
