using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace _10_Completion_Search_Plugin.plugins.Search
{
    public sealed class AzureSearchPlugin(string searchServiceEndpoint, string indexName, string apiKey) : IDisposable
    {
        private readonly SearchClient _searchClient = new(new Uri(searchServiceEndpoint), indexName, new AzureKeyCredential(apiKey));

        private bool _disposed = false;



        [KernelFunction, Description("Perform a search using Azure AI Search.")]
        public async Task<string> SearchAsync(
            [Description("Search query")] string query,
            [Description("Number of results")] int count = 20,
            [Description("Number of results to skip")] int offset = 0,
            CancellationToken cancellationToken = default
            )
        {
            var options = new SearchOptions
            {
                Size = count,
                Skip = offset
            };

            var response = await _searchClient.SearchAsync<SearchDocument>(query, options, cancellationToken);
            var documents = response.Value.GetResultsAsync().Select(result => result.Document);

            var results = new List<SearchResultItem>();

            await foreach (var document in documents)
            {
                results.Add(new SearchResultItem
                {
                    Title = document.GetString("title") ?? "",
                    Name = document.GetString("name") ?? "",
                    Url = document.GetString("url") ?? "",
                    Snippet = document.GetString("snippet") ?? "",
                    DatePublished = document.GetDateTimeOffset("date_published")?.DateTime ?? DateTime.MinValue,
                    PageContent = document.GetString("page_content") ?? ""
                });
            }

            return JsonSerializer.Serialize(results);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // If you add disposable resources in the future, dispose them here.
                // Currently, SearchClient does not require disposal.
                _disposed = true;
            }
        }
    }
}
