using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace _05_Planners_Handlebars.HandlebarsFunctions
{
    public class HandlebarsPlannerFunction
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

                var roleTalkPluginName = "RoleTalk";

                var scientistResponseFunctionName = "ScientistResponse";
                var policemanResponseFunctionName = "PolicemanResponse";

                var scientistResponseFunction = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
                {
                    Name = scientistResponseFunctionName,
                    Description = "Respond as if you were a Scientist",
                    Template = @"After the user request/question, 
                    {{$input}},
                    Respond to the user question as if you were a Scientist. 
                    Respond to it as you were him, showing your personality",
                    TemplateFormat = "semantic-kernel",
                    InputVariables = [
                        new() { Name = "input" }
                    ]
                });

                var policemanResponseFunction = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
                {
                    Name = policemanResponseFunctionName,
                    Description = "Respond as if you were a Policeman",
                    Template = @"After the user request/question, 
                    {{$input}},
                    Respond to the user question as if you were a Policeman. 
                    Respond to it as you were him, showing your personality",
                    TemplateFormat = "semantic-kernel",
                    InputVariables = [
                        new() { Name = "input" }
                    ]
                });

                var roleOpinionsPlugin = KernelPluginFactory.CreateFromFunctions(
                    pluginName: roleTalkPluginName,
                    description: "Responds to questions or statements assuming different roles.",
                    functions : [scientistResponseFunction, policemanResponseFunction]
                );

                kernel.Plugins.Add(roleOpinionsPlugin);

                string userQuestion = "I am being attacked by a thug which wants to rob me, what do the experts recommend me to do in my position? I am weak, no combat skills and not a good runner...";

                string planPrompt = @$"This is the user question to my expert friends:
            ---
            User Question:
            {userQuestion}
            ---
            Please take this question as input for getting the expert opinions, Mr. Policeman, Scientist suggestions. Do not modify the input.
            Use the plugin {roleTalkPluginName} to get the suggestions and opinions of the experts.
            In addition state each expert opinion on each other stated opinions.
            Put the expert responses preceded with EXPERT SUGGESTIONS: and inside that preceed with Policeman: and Scientist: for clarity.
            Perform this with the following steps:
            1. Get the suggestions from each the experts.
            2. Get the opinions of each expert on the other expert suggestions.
            3. Return the results in the format:
            Expert SUGGESTIONS: Policeman: <suggestion> Scientist: <suggestion> 
            OPINIONS: Policeman: <opinion on Scientist> Scientist: <opinion on Policeman> 
            IMPORTANT: on the plan ensure that the user question is assigned to a variable and used as input. Do not modify the user question input.";

                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                var arguments = new KernelArguments(executionSettings)
                {
                    { "userQuestion", userQuestion }
                };

                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("=== EXECUTING PLAN ===");
                Console.WriteLine("Available Functions:");
                foreach (var plugin in kernel.Plugins)
                {
                    Console.WriteLine($"Plugin: {plugin.Name}");
                    foreach (var function in plugin)
                    {
                        Console.WriteLine($"\t- {function.Name}: {function.Description}");
                    }
                }
                Console.WriteLine("\nUser Question:");
                Console.WriteLine(userQuestion);

                var result = await kernel.InvokePromptAsync(planPrompt, arguments);

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
