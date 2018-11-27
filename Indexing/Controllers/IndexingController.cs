using System;
using System.Collections.Generic;
using System.Linq;
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


        [Route("transaction")]
        [HttpPost]
        public async Task<object> EsHealth([FromBody] Transaction t)
        {
            try
            {
                await LogAsync(t);
                return true;
            }
            catch (Exception ex)
            {

                return $"{ex.Message}, inner:{ex.InnerException} stk:{ex.StackTrace}";
            }

        }

        private Task LogAsync(Transaction transaction)
        {
            return Task.Run(() =>
            {
                _esClient.IndexAsync(transaction);
            });
        }
    }
}
