using Nest;
using System;

namespace Indexing
{
    public class EsClient
    {
        private IElasticClient _client;

        public EsClient()
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable("USE_INDEXING"), out var useEs) && useEs)
            {
                var settings = new ConnectionSettings(new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_URL"))).DefaultIndex("demo-cko");
                var client = new ElasticClient(settings);

                _client = client;
            }
        }

        public void IndexAsync(object obj)
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable("USE_INDEXING"), out var useEs) && useEs)
                _client.IndexDocumentAsync(obj);
        }

    }
}
