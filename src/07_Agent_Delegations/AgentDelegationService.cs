using _07_Agent_Delegations.Plugins;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace _07_Agent_Delegations
{
    public class AgentDelegationService
    {
        private readonly List<ChatCompletionAgent> _agents = [];
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

                var menuPlugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
                kernel.Plugins.Add(menuPlugin);

                var policeAgentPath = Path.Combine(Directory.GetCurrentDirectory(), "Agents", "PolicemanAgent.yaml");
                var toolAgentPath = Path.Combine(Directory.GetCurrentDirectory(), "Agents", "ToolAgent.yaml");

                var service = new AgentDelegationService();

                await service.AddYamlAgent(kernel, policeAgentPath);
                await service.AddYamlAgent(kernel, toolAgentPath);

                var menuAgent = new ChatCompletionAgent
                {
                    Name = "MenuAgent",
                    Instructions = "You are a helpful assistant that helps users choose the right agent for their requests based on a provided menu of agents and their descriptions. " +
                                   "When a user makes a request, analyze it carefully and select the most appropriate agent from the menu to handle the request effectively. " +
                                   "Respond with the name of the selected agent only.",
                    Kernel = kernel,
                    Description = "An agent that selects the appropriate agent based on user requests and a menu of available agents.",
                    Arguments = new KernelArguments(new PromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    })
                };

                service.AddAgent(menuAgent);

                var messages = new string[]
                {
                      "What is on today's menu?",
                      "how much does the Eye Steak with veggies cost? ",
                      "There's shooting here!",
                      "Thank you",
                };

                // ---------- rendering chat with agents ----------

                foreach (var agent in service._agents)
                {
                    foreach (var message in messages)
                    {
                        await ChatWithAgent(agent, message);
                    }
                    Console.WriteLine(new string('-', 50));
                }


                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private async Task AddYamlAgent(Kernel kernel, string yamlPath)
        {

            if (!File.Exists(yamlPath))
            {
                Console.WriteLine($"YAML file not found at: {yamlPath}");
                return;
            }

            var yamlContent = await File.ReadAllTextAsync(yamlPath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

            var agentName = yamlData.ContainsKey("name") ? yamlData["name"].ToString() : "YamlAgent";
            var description = yamlData.ContainsKey("description") ? yamlData["description"].ToString() : "";
            var template = yamlData.ContainsKey("template") ? yamlData["template"].ToString() : "";

            var yamlAgent = new ChatCompletionAgent
            {
                Name = agentName!,
                Instructions = template!,
                Kernel = kernel,
                Description = description!
            };

            AddAgent(yamlAgent);
        }

        private static async Task ChatWithAgent(ChatCompletionAgent agent, string userMessage)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userMessage);

            Console.WriteLine($"User: {userMessage}");

            await foreach (ChatMessageContent message in agent.InvokeAsync(chatHistory))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{agent.Name}: {message.Content}");
                Console.ForegroundColor = ConsoleColor.White;

                chatHistory.Add(message);
            }
        }

        private void AddAgent(ChatCompletionAgent agent)
        {
            _agents.Add(agent);
        }

    }
}
