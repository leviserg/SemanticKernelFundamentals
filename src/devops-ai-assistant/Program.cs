using devops_ai_assistant;
using devops_ai_assistant.Filters;
using devops_ai_assistant.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
.AddJsonFile(filePath)
.Build();

// Set your values in appsettings.json
string apiKey = config["AZURE_OPENAI_KEY"]!;
string endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
string deploymentName = config["DEPLOYMENT_NAME"]!;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = builder.Build();

kernel.ImportPluginFromType<DevopsPlugin>();

var settings = new OpenAIPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

IChatCompletionService chatCompletionService = null!;
try
{
    chatCompletionService = kernel.GetRequiredService<IChatCompletionService>()!;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error getting chat completion service: " + ex.Message);
    Console.ResetColor();
    Console.ReadKey();
    return;
}

ChatHistory chatHistory = [];

#region Create function from prompt

var promptFunctionName = Constants.DeployStageFunctionName;

var promptFunction = kernel.CreateFunctionFromPrompt(
    promptTemplate: @"This is the most recent build log:
    

 If there are errors, do not deploy the stage environment. Otherwise, invoke the stage deployment function",
    functionName: promptFunctionName,
    description: "Deploy Staging Environment"
    );

kernel.Plugins.AddFromFunctions(
    pluginName: promptFunctionName,
    [ promptFunction]
);

#endregion

#region Create function from prompt with Handlebars template

var hbPromptTemplateFunctionName = Constants.BranchPluginName;

string hbPrompt = """
     <message role="system">Instructions: Before creating a new branch for a user, request the new branch name and base branch name/message>
     <message role="user">Can you create a new branch?</message>
     <message role="assistant">Sure, what would you like to name your branch? And which base branch would you like to use?</message>
     <message role="user"></message>
     <message role="assistant">
     """;

var templateFactory = new HandlebarsPromptTemplateFactory();

var hbConfig = new PromptTemplateConfig()
{
    Template = hbPrompt,
    TemplateFormat = "handlebars",
    Name = Constants.CreateBranchFunctionName
};

var hbTemplateFunction = kernel.CreateFunctionFromPrompt(
    promptConfig: hbConfig,
    promptTemplateFactory: templateFactory
    );

var branchPlugin = kernel.CreatePluginFromFunctions(
    hbPromptTemplateFunctionName,
    [ hbTemplateFunction]
);

kernel.Plugins.Add(branchPlugin);

#endregion


#region Add filters

kernel.FunctionInvocationFilters.Add(new PermissionFilter());

#endregion

#region user interaction
Console.WriteLine("Press enter to exit");
Console.WriteLine("Assistant: How may I help you?");
Console.Write("User: ");

string input = Console.ReadLine()!;

while (input != "")
{
    chatHistory.AddUserMessage(input);
    await GetReply();
    input = GetInput();
}

string GetInput()
{
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    chatHistory.AddUserMessage(input);
    return input;
}

async Task GetReply()
{
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    chatHistory.AddAssistantMessage(reply.ToString());
}
#endregion