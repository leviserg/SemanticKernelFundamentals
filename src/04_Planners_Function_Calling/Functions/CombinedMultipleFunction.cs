using _04_Planners_Function_Calling.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace _04_Planners_Function_Calling.Functions
{
    public class CombinedMultipleFunction
    {

        public static async Task Execute()
        {
            try
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

                var scientistResponseFunctionName = "ScientistResponse";
                var policemanResponseFunctionName = "PolicemanResponse";

                var scientistResponseFunction = KernelFunctionFactory.CreateFromPrompt(
                    promptTemplate: "Respond to the user question as if you were a University Professor. Respond to it as you were him, showing your personality",
                    functionName: scientistResponseFunctionName,
                    description: "Responds to a question as a Scientist and University Professor."
                );

                var policemanResponseFunction = KernelFunctionFactory.CreateFromPrompt(
                    promptTemplate: "Respond to the user question as if you were a Policeman. Respond to it as you were him, showing your personality, humor and level of intelligence.",
                    functionName: policemanResponseFunctionName,
                    description: "Responds to a question as a Policeman."
                );

                // ###### IMPORTANT: Register the functions in the kernel

                var roleTalkPluginName = "RoleTalk";

                KernelPlugin roleOpinionsPlugin = KernelPluginFactory.CreateFromFunctions(
                    pluginName: roleTalkPluginName,
                    description: "Responds to questions or statements assuming different roles.",
                    functions: [ scientistResponseFunction, policemanResponseFunction ]
                );

                kernel.Plugins.Add(roleOpinionsPlugin);
                kernel.Plugins.AddFromType<DatePlugin>();

                // ###### END IMPORTANT

                string userPrompt = @$"I just woke up and found myself in the middle of nowhere, 
    do you know what date is it? and what would a policeman and a scientist do in my place?
    Please provide me the date using the {nameof(DatePlugin)} plugin and the {nameof(DatePlugin.GetCurrentDate)} function, and then 
    the responses from the policeman and the scientist, on this order. 
    For this two responses, use the {roleTalkPluginName} plugin and the {policemanResponseFunctionName} and {scientistResponseFunctionName} functions.";

                // ###### Push settings in KernelArguments

                var settings = new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                var arguments = new KernelArguments(settings);

                // ###### InvokeInvokePromptAsync != InvokeAsync

                var result = await kernel.InvokePromptAsync(
                    promptTemplate: userPrompt,
                    arguments: arguments);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"AI says: {result}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
