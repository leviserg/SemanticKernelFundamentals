using Microsoft.SemanticKernel;

namespace _00_Prompt_Plugins_Basic
{
    public class PromptFunction
    {
        private const string textSample = "Effective prompt design is essential to achieving desired outcomes with LLM AI models. Prompt engineering, also known as prompt design, is an emerging field that requires creativity and attention to detail. It involves selecting the right words, phrases, symbols, and formats that guide the model in generating high-quality and relevant texts.\r\n\r\nIf you've already experimented with ChatGPT, you can see how the model's behavior changes dramatically based on the inputs you provide. For example, the following prompts produce very different outputs:";

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

            var kernel = builder.Build();

            var response = await GetTextSummaryAsync(kernel);//await GetSimpleResponseAsync(kernel);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Response:");
            Console.WriteLine(response);
            Console.ResetColor();
        }
        private static async Task<string> GetTextSummaryAsync(Kernel kernel)
        {
            try
            {

                var pluginsDirectoryName = "Plugins";

                var summarizePluginDirectoryName = "SummarizePlugin";

                var pluginsDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    pluginsDirectoryName);

                kernel.ImportPluginFromPromptDirectory(pluginsDirectory);

                KernelArguments arguments = new()
                {
                    {
                        "input", textSample
                    }
                };

                var result = await kernel.InvokeAsync(
                    pluginsDirectoryName,
                    summarizePluginDirectoryName,
                    arguments
                );

                return result.GetValue<string>() ?? "No response";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";

            }
        }

        private static async Task<string> GetSimpleResponseAsync(Kernel kernel)
        {
            var prompt = "Enlist GRASP patterns.";
            var promptFunction = kernel.CreateFunctionFromPrompt(prompt);

            var result = await kernel.InvokeAsync(promptFunction);

            return result.GetValue<string>() ?? "No response";
        }
    }
}
