using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;


namespace _06_Craft_Agents_CodeAndYaml
{
    /// <summary>
    /// Advanced agent implementation showing multiple patterns in SK 1.68
    /// </summary>
    public class AdvancedAgentService
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

                // Example 1: Simple ChatCompletionAgent (replaces AgentBuilder)
                var pirateAgent = new ChatCompletionAgent
                {
                    Name = "PirateParrot",
                    Instructions = """
                        You are a pirate parrot. 
                        Repeat what the user says in pirate speak.
                        End every response with "SQUAWK! SQUAWK!"
                        """,
                    Kernel = kernel,
                    Description = "A pirate-speaking parrot agent"
                };

                // Example 2: Agent with execution settings
                var professionalAgent = new ChatCompletionAgent
                {
                    Name = "ProfessionalAssistant",
                    Instructions = """
                        You are a professional business assistant.
                        Provide clear, concise, and professional responses.
                        Always maintain a formal tone.
                        """,
                    Kernel = kernel,
                    Description = "A professional business assistant",
                    Arguments = new KernelArguments()
                    {
                        // Can add execution settings here
                    }
                };

                // Test agents
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.WriteLine("        SEMANTIC KERNEL 1.68 AGENT EXAMPLES        ");
                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;

                await DemoAgent(pirateAgent, "Hello, how are you today?");
                Console.WriteLine();
                
                await DemoAgent(professionalAgent, "Can you help me write an email to my team?");
                Console.WriteLine();

                // Example 3: Multi-turn conversation
                await MultiTurnConversation(pirateAgent);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async Task DemoAgent(ChatCompletionAgent agent, string userMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"═══ {agent.Name} ═══");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Description: {agent.Description}");
            Console.WriteLine();

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userMessage);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"👤 User: {userMessage}");
            Console.ForegroundColor = ConsoleColor.White;

            await foreach (ChatMessageContent message in agent.InvokeAsync(chatHistory))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"🤖 {agent.Name}: {message.Content}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async Task MultiTurnConversation(ChatCompletionAgent agent)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"═══ Multi-Turn Conversation with {agent.Name} ===");
            Console.ForegroundColor = ConsoleColor.White;

            var chatHistory = new ChatHistory();

            var messages = new[]
            {
                "Tell me a short story about treasure",
                "What happened to the treasure?",
                "Did anyone find it?"
            };

            foreach (var userMessage in messages)
            {
                chatHistory.AddUserMessage(userMessage);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"👤 User: {userMessage}");
                Console.ForegroundColor = ConsoleColor.White;

                await foreach (ChatMessageContent message in agent.InvokeAsync(chatHistory))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"🤖 {agent.Name}: {message.Content}");
                    Console.ForegroundColor = ConsoleColor.White;
                    
                    chatHistory.Add(message);
                }

                Console.WriteLine();
            }
        }
    }
}
