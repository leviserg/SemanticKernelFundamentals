using _10_Completion_Search_Plugin.plugins.Search;
using Microsoft.SemanticKernel;

namespace _10_Completion_Search_Plugin
{
    public static class SearchService
    {

        private static readonly string SearchResultsFileName = "searchResults.json";
        //private static readonly string ResearchReportFileName = "ResearchReport.txt";

        public static async Task Execute()
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
                var name = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

                string searchServiceEndpoint = Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_ENDPOINT")!;
                string indexName = Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_INDEX_NAME")!; ;
                string searchApiKey = Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_API_KEY")!;

                var builder = Kernel.CreateBuilder();

                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: name,
                    endpoint: endpoint,
                    apiKey: apiKey);

                var kernel = builder.Build();

                using var webSearchEnginePlugin = new AzureSearchPlugin(searchServiceEndpoint, indexName, searchApiKey);

                var webSearchEnginePluginName = webSearchEnginePlugin.GetType().Name;
                kernel.ImportPluginFromObject(webSearchEnginePlugin, webSearchEnginePluginName);

                var topicOfResearch = "What are the latest generative AI models and advancements for the last week?";

                string resultsJson = await webSearchEnginePlugin.SearchAsync(topicOfResearch);
                await File.WriteAllTextAsync(SearchResultsFileName, resultsJson);

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
