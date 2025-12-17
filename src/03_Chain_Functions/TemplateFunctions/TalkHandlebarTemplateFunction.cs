using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace _03_Chain_Functions.TemplateFunctions
{
    public class TalkHandlebarTemplateFunction
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

                var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "RoleTalk");

                kernel.ImportPluginFromPromptDirectory(pluginsDirectory);

                //string question = "What are the GRASP patterns?";
                string question = "What's the best way to deal with a city-wide power outage?";

                var chainingFunctionsWithHandlebarsFunction = kernel.CreateFunctionFromPrompt(
                    new()
                    {
                        Template = @"
                        {{set ""responseAsPoliceman"" (RoleTalk-Policeman input) }}
                        {{set ""responseAsScientific"" (RoleTalk-Scientist input) }}
                        {{set ""opinionFromScientificToPoliceman"" (RoleTalk-Scientist responseAsPoliceman) }}

                        {{!-- Example of concatenating text and variables to finally output it with json --}}
                        {{set ""finalOutput"" 
                               (concat  "" Policeman:               "" responseAsPoliceman 
                                        "" Scientific:              "" responseAsScientific  
                                        "" Scientific to Policeman: "" opinionFromScientificToPoliceman)
                        }}
                
                        Output the following responses as is, do not modify anything:
                        {{json finalOutput}}
                        ",
                        TemplateFormat = "handlebars"
                    },
                    new HandlebarsPromptTemplateFactory()
                );

                var arguments = new KernelArguments
                {
                    { "input", question }
                };
                var result = await kernel.InvokeAsync(chainingFunctionsWithHandlebarsFunction, arguments);

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
