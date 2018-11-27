using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Indexing.Controllers
{
    [Route("api/[controller]")]
    public class IndexingController : Controller
    {

        private EsClient _esClient;

        public IndexingController(EsClient eSClient)
        {
            _esClient = eSClient;
        }

        [Route("health")]
        public async Task<string> EsHealth()
        {
            try
            {
                var esUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(esUrl);

                string content = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => content);
            }
            catch (Exception ex)
            {

                return $"{ex.Message}, inner:{ex.InnerException} stk:{ex.StackTrace}";
            }
        }

        [Route("transaction")]
        [HttpPost]
        public async Task<object> Authorise([FromBody] Transaction t)
        {
            try
            {
                var res = await LogAsync(t);
                return res;
            }
            catch (Exception ex)
            {

                return $"{ex.Message}, inner:{ex.InnerException} stk:{ex.StackTrace}";
            }

        }

        private async Task<object> LogAsync(Transaction transaction)
        {
            var res = await _esClient.IndexAsync(transaction);
            return res;
        }
    }
}
