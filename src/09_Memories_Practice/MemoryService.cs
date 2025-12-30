using Microsoft.KernelMemory.AI.AzureOpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace _09_Memories_Practice
{
    public class MemoryService
    {
        //private const string MemoryCollectionName = "aboutMe";

        public static async Task Execute()
        {
            /* ========= NOT PRODUCTION READY CODE =========
             * This code is for learning purposes only.
             * The library is under evaluation and may change.
             * =======================================
            try
            {
                // Get environment variables
                var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
                var chatDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;
                var embeddingDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME")!;
                var memoryCollectionName = "my-memories";

                // Create kernel and add Azure OpenAI chat completion
                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: chatDeploymentName,
                    endpoint: endpoint,
                    apiKey: apiKey);

                var kernel = builder.Build();

                // Set up memory store and text memory with Azure OpenAI embeddings
                IMemoryStore memoryStore = new VolatileMemoryStore();

                var embeddingGenerator = new AzureOpenAITextEmbeddingGenerator(
                    deploymentName: embeddingDeploymentName,
                    endpoint: endpoint,
                    apiKey: apiKey);

                ISemanticTextMemory textMemory = new SemanticTextMemory(memoryStore, embeddingGenerator);

                // Save some memories
                await textMemory.SaveInformationAsync(
                    collection: memoryCollectionName,
                    text: "I live in Zurich.",
                    id: "info5");

                await textMemory.SaveInformationAsync(
                    collection: memoryCollectionName,
                    text: "I love learning, AI, XR and complex challenges.",
                    id: "info6");

                // Recall memories
                string ask = "What do I love?";
                Console.WriteLine($"Ask: {ask}");

                var results = textMemory.SearchAsync(
                    collection: memoryCollectionName,
                    query: ask,
                    limit: 2,
                    minRelevanceScore: 0.79);

                await foreach (var memory in results)
                {
                    Console.WriteLine($"Answer: {memory.Metadata.Text}");
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            */
        }
    }

}
