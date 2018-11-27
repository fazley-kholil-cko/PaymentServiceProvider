using Nest;
using System;
using System.Threading.Tasks;

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

        public async Task<object> IndexAsync(object obj)
        {
          
                var res = await _client.IndexDocumentAsync(obj);
                return res;
            
               
        }

    }
}
