using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace _08_Agent_Collaborations
{
    public class AgentCollaborationService : IDisposable
    {

        readonly List<ChatCompletionAgent> _agents = [];
        
        #pragma warning disable SKEXP0110
        private AgentGroupChat? _agentGroupChat;
#pragma warning restore SKEXP0110

        private const bool UseCoordinator = false;

        private const string Separator = "═══════════════════════════════════════════════════";

        public async Task Execute()
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

                // Create agents
                // Marketing Editor agent => Reviews slogan, provides feedback and gives the FINAL APPROVAL
                var editorAgent =
                    Track(new ChatCompletionAgent
                    {
                        Name = "MarketingEditor",
                        Instructions = "You are a professional editor with a profound expertise in crafting and refining content for marketing. You are deeply passionate about the intersection of technology and storytelling and love when words ryhme together. Your goal is to determine if a marketing slogan is acceptable, even if it isn't perfect.  If not, provide constructive insights on how to improve the slogan without providing an example.  Respond to the most recent message by evaluating and providing feedback without giving any example.  Always repeat the slogan at the beginning.  If the slogan is is acceptable and meets your criteria, say: I APPROVE.",
                        Kernel = kernel,
                        Description = "Marketing Editor"
                    });


                // Marketing Writer Agent => generates ideas
                var writerAgent =
                    Track(new ChatCompletionAgent
                    {
                        Name = "MarketingWriter",
                        Instructions = "You are a marketing writer with some years of experience, you like efficiency of words and sarcasm. You like to deliver greatness and do your outmost always. Your goal is given an idea description to provide a Marketing slogan. If feedback is provided, take it into consideration to improve the Slogan.",
                        Kernel = kernel,
                        Description = "Marketing Writer"
                    });

                // Create coordinator agent to oversee collaboration
                var coordinatorAgent =
                    Track(new ChatCompletionAgent
                    {
                        Name = "MarketingCoordinator",
                        Instructions = @"You are a coordinator managing the collaboration between a Marketing Writer and a Marketing Editor. 
                                      Your job is to: 
                                      1. First, pass the initial concept to the Marketing Writer to create a slogan. 
                                      2. Then, send the slogan to the Marketing Editor for review. 
                                      3. If the editor provides feedback (and doesn't approve), relay that feedback to the writer for improvement. 
                                      4. Continue this process until the Marketing Editor says 'I APPROVE'. 
                                      Always maintain context by including the original concept and any previous feedback in your communications.",
                        Kernel = kernel,
                        Description = "Marketing Coordinator"
                    });

                string ideaToEllaborate = @"concept: AI Agents that can write twitter and LinkedIn
                     messages and blog posts with the style of an author.";

                if (UseCoordinator)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(Separator);
                    Console.WriteLine("     USING COORDINATOR AGENT WITH GROUP CHAT       ");
                    Console.WriteLine(Separator);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    #pragma warning disable SKEXP0110
                    _agentGroupChat = new AgentGroupChat(writerAgent, editorAgent);
                    #pragma warning restore SKEXP0110

                    _agentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, ideaToEllaborate));
                    Console.WriteLine($"# User: {ideaToEllaborate}");
                    Console.WriteLine();

                    bool isComplete = false;
                    int maxIterations = 10;
                    int iteration = 0;

                    do
                    {
                        #pragma warning disable SKEXP0110
                        await foreach (var message in _agentGroupChat.InvokeAsync())
                        #pragma warning restore SKEXP0110
                        {
                            DisplayMessage(message);
                            
                            if (message.Content?.Contains("I APPROVE", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                isComplete = true;
                                break;
                            }
                        }

                        iteration++;
                        if (iteration >= maxIterations)
                        {
                            Console.WriteLine("Maximum iterations reached.");
                            isComplete = true;
                        }
                    }
                    while (!isComplete);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(Separator);
                    Console.WriteLine("     MANUAL AGENT COORDINATION (NO COORDINATOR)    ");
                    Console.WriteLine(Separator);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    #pragma warning disable SKEXP0110
                    _agentGroupChat = new AgentGroupChat();
                    #pragma warning restore SKEXP0110

                    _agentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, ideaToEllaborate));
                    Console.WriteLine($"# User: {ideaToEllaborate}");
                    Console.WriteLine();

                    bool isComplete = false;
                    do
                    {
                        #pragma warning disable SKEXP0110
                        await foreach (var message in _agentGroupChat.InvokeAsync(writerAgent))
                        #pragma warning restore SKEXP0110
                        {
                            DisplayMessage(message);
                        }

                        #pragma warning disable SKEXP0110
                        await foreach (var message in _agentGroupChat.InvokeAsync(editorAgent))
                        #pragma warning restore SKEXP0110
                        {
                            DisplayMessage(message);
                            
                            if (message.Content?.Contains("I APPROVE", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                isComplete = true;
                            }
                        }
                    }
                    while (!isComplete);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                CleanUp();
            }
        }

        private ChatCompletionAgent Track(ChatCompletionAgent agent)
        {
            _agents.Add(agent);

            return agent;
        }

        private static void DisplayMessage(ChatMessageContent message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{message.Role}]");
            
            if (!string.IsNullOrEmpty(message.AuthorName))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"# {message.Role}: ({message.AuthorName})");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message.Content);
            }
            else
            {
                Console.WriteLine($"# {message.Role}: {message.Content}");
            }
            Console.WriteLine();
        }

        private void CleanUp()
        {

            if (_agents.Count > 0)
            {
                _agents.Clear();
            }

            _agentGroupChat = null;

            Console.WriteLine("Cleaned up agents and threads.");
        }

        public void Dispose()
        {
            _agents?.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
