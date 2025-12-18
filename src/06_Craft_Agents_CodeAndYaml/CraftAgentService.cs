using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace _06_Craft_Agents_CodeAndYaml
{
    public class CraftAgentService
    {
        readonly List<ChatCompletionAgent> _agents = [];

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

                var service = new CraftAgentService();
                
                // Create agent from code
                await service.AddCodeAgent(kernel);
                
                // Create agent from YAML
                await service.AddYamlAgent(kernel);

                // Display all agents
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══ Available Agents ═══");
                foreach (var agent in service._agents)
                {
                    Console.WriteLine($"Agent: {agent.Name}");
                    Console.WriteLine($"\tDescription: {agent.Description}");
                    Console.WriteLine($"\tInstructions: {agent.Instructions}");
                    Console.WriteLine();
                }
                Console.ForegroundColor = ConsoleColor.White;

                // Test the code agent
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("═══ Testing Code Agent (Pirate Parrot) ═══");
                Console.ForegroundColor = ConsoleColor.White;
                
                await ChatWithAgent(service._agents[0], "Hello, how are you today?");

                Console.WriteLine();

                // Test the YAML agent
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("═══ Testing YAML Agent (Policeman) ═══");
                Console.ForegroundColor = ConsoleColor.White;
                
                await ChatWithAgent(service._agents[1], "My neighbor's dog keeps barking all night and I can't sleep.");

                service.Clear();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private async Task AddCodeAgent(Kernel kernel)
        {
            var codeAgent = new ChatCompletionAgent
            {
                Name = "CodeParrot",
                Instructions = "Repeat the user message in the voice of a pirate " +
                              "and then end with parrot sounds.",
                Kernel = kernel,
                Description = "A fun chat bot that repeats the user message in the" +
                             " voice of a pirate."
            };

            _agents.Add(codeAgent);

            await Task.CompletedTask;
        }

        private async Task AddYamlAgent(Kernel kernel)
        {
            var yamlPath = Path.Combine(Directory.GetCurrentDirectory(), "Agents", "PolicemanAgent.yaml");
            
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

            _agents.Add(yamlAgent);
        }

        /// <summary>
        /// Chat with a specific agent
        /// </summary>
        private static async Task ChatWithAgent(ChatCompletionAgent agent, string userMessage)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userMessage);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"User: {userMessage}");
            Console.ForegroundColor = ConsoleColor.White;

            await foreach (ChatMessageContent message in agent.InvokeAsync(chatHistory))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{agent.Name}: {message.Content}");
                Console.ForegroundColor = ConsoleColor.White;
                
                chatHistory.Add(message);
            }
        }

        public void Clear()
        {
            _agents.Clear();
        }
    }
}
