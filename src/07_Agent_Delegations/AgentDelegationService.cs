using _07_Agent_Delegations.Plugins;
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

                var menuAgent = new ChatCompletionAgent
                {
                    Name = "MenuAgent",
                    Instructions = $"Use instructions from available plugin {nameof(MenuPlugin)}-{nameof(MenuPlugin.GetMenu)} and {nameof(MenuPlugin)}-{nameof(MenuPlugin.GetItemPrice)}.",
                    Kernel = kernel,
                    Description = "Gets menu info."
                };

                service.AddAgent(menuAgent);

                var toolAgent = await service.AddYamlAgent(kernel, toolAgentPath);

                var messages = new string[]
                {
                      "What is on today's menu?",
                      "how much does the Eye Steak with veggies cost? ",
                      "There's shooting here!",
                      "Thank you",
                };

                foreach (var message in messages)
                {
                    await service.ChatWithDelegation(toolAgent!, message);
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

        private async Task<ChatCompletionAgent> AddYamlAgent(Kernel kernel, string yamlPath)
        {

            if (!File.Exists(yamlPath))
            {
                throw new Exception($"YAML file not found at: {yamlPath}");
            }

            var yamlContent = await File.ReadAllTextAsync(yamlPath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

            var agentName = yamlData.ContainsKey("name") ? yamlData["name"].ToString() : "YamlAgent";
            var description = yamlData.ContainsKey("description") ? yamlData["description"].ToString() : "";
            var template = yamlData.ContainsKey("template") ? yamlData["template"].ToString() : "";

            var yamlAgent = new ChatCompletionAgent()
            {
                Name = agentName!,
                Instructions = template!,
                Kernel = kernel,
                Description = description!
            };

            AddAgent(yamlAgent);

            return yamlAgent;
        }

        private async Task ChatWithDelegation(ChatCompletionAgent agent, string userMessage)
        {
            Console.WriteLine($"\nUser: {userMessage}");

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userMessage);

            await foreach (ChatMessageContent message in agent.InvokeAsync(chatHistory))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{agent.Name}: {message.Content}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void AddAgent(ChatCompletionAgent agent)
        {
            _agents.Add(agent);
        }

    }
}
