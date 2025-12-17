using _02_Prompt_Templates_Handlebars.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace _02_Prompt_Templates_Handlebars.Templates
{
    public class HandlebarPromptTemplateFunction
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

            builder.Plugins.AddFromType<TimePlugin>();

            var kernel = builder.Build();

            string[] todaysCalendar =
            [
                "8am - wakeup",
                "9am - work",
                "12am - lunch",
                "1pm - work", 
                "6pm - exercise",
                "7pm - study", 
                "10pm - sleep"
            ];

            var handlebarsTemplate = @"
                    Please explain in a fun way the day agenda
                    {{ set ""dayAgenda"" (todaysCalendar)}}
                    {{ set ""whatTimeIsIt"" (" + nameof(TimePlugin) + "-" + nameof(TimePlugin.GetCurrentTime) + @") }}
                    {{#each dayAgenda}}
                        Explain what you are doing at {{this}} in a fun way.
                    {{/each}}

                    Explain what you will be doing next at {{whatTimeIsIt}} in a fun way.";


            var handlebarsFunction = kernel.CreateFunctionFromPrompt(
                new PromptTemplateConfig()
                {
                    Template = handlebarsTemplate,
                    TemplateFormat = "handlebars",
                    AllowDangerouslySetContent = true
                },
                new HandlebarsPromptTemplateFactory()
            );

            var arguments = new KernelArguments
            {
                { "todaysCalendar", todaysCalendar }
            };

            var result = await kernel.InvokeAsync(handlebarsFunction, arguments);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(result);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
