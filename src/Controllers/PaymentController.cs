using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentServiceProvider.Indexing;
using PaymentServiceProvider.Models;

namespace PaymentServiceProvider.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        public static string ApplicationEnvironment => Environment.GetEnvironmentVariable("ENVIRONMENT");
        public static string Version => Environment.GetEnvironmentVariable("VERSION");
        public static int Latency => int.Parse(Environment.GetEnvironmentVariable("LATENCY"));
        public static decimal ErrorRate => decimal.Parse(Environment.GetEnvironmentVariable("ERROR_RATE"));
        public static decimal ErrorRateOverTotalHit => decimal.Parse(Environment.GetEnvironmentVariable("ERROR_RATE_OVER_TOTAL_HIT"));


        public static decimal FailureRate => (ErrorRate / 100) * ErrorRateOverTotalHit;
        public static decimal SuccessRate => ErrorRateOverTotalHit - FailureRate;

        public static decimal SuccessHitCount { get; set; } = 1;
        public static decimal FailureHitCount { get; set; } = 1;

        private EsClient _esClient;

        public PaymentController(EsClient eSClient)
        {
            _esClient = eSClient;
        }

        [Route("es")]
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

        [Route("es/ing")]
        public async Task<string> EsHealthIngress()
        {
            try
            {
                var esUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
                var l5d = Environment.GetEnvironmentVariable("L5D_INGRESS_LB");



                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Host", "Host: 139.59.177.30:9200");
                var response = await httpClient.GetAsync(l5d);

                string content = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => content);
            }
            catch (Exception ex)
            {

                return $"{ex.Message}, inner:{ex.InnerException} stk:{ex.StackTrace}";
            }

        }

        [Route("es/google")]
        public async Task<string> EsHealthGoogles([FromQuery] string host)
        {
            try
            {
                var esUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
                var l5d = Environment.GetEnvironmentVariable("L5D_INGRESS_LB");



                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Host", $"Host: {host}");
                var response = await httpClient.GetAsync(l5d);

                string content = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => content);
            }
            catch (Exception ex)
            {

                return $"{ex.Message}, inner:{ex.InnerException} stk:{ex.StackTrace}";
            }

        }


        [Route("authorise")]
        public async Task<IActionResult> AuthorisePayment()
        {
            var startTime = Environment.TickCount;

            try
            {
                //similate error 500 out of a total count of transaction
                if (ErrorRate > 0)
                {
                    if (SuccessHitCount > SuccessRate)
                    {
                        if (FailureHitCount == FailureRate)
                        {
                            SuccessHitCount = 1; //reset 
                            FailureHitCount = 1; //reset
                        }


                        FailureHitCount++;

                        //simulate server is down 
                        throw new Exception("Server not available");
                    }
                }

                //simulate call to external payment processor
                var response = await AuthoriseAsync();

                //save the transaction to es
                await LogAsync(response);

                response.Metric.TotalExecution = Environment.TickCount - startTime;
                response.Metric.ConsecutiveHitCount = SuccessHitCount;
                SuccessHitCount++;

                return await Task.Run(() => Ok(response));
            }
            catch (Exception ex)
            {
                //log the error
                await LogAsync(ex, startTime);

                throw;
            }
        }

        private Task<Transaction> AuthoriseAsync()
        {
            Thread.Sleep(Latency);

            return Task.Run(() => new Transaction
            {
                Environment = ApplicationEnvironment,
                Version = Version,
                ResponseCode = "Approved",
                IsSuccess = true,
                TransactionDate = DateTime.Now,
                Metric = new Metric()
            });
        }

        private Task LogAsync(Exception ex, int startTime)
        {
            return Task.Run(() =>
            {
                var errorTransactionLog = new Transaction
                {
                    Environment = ApplicationEnvironment,
                    Version = Version,
                    ResponseCode = "Error",
                    IsSuccess = true,
                    TransactionDate = DateTime.Now,
                    Metric = new Metric
                    {
                        ConsecutiveHitCount = FailureHitCount,
                        TotalExecution = Environment.TickCount - startTime
                    },
                    Exception = ex
                };

                _esClient.IndexAsync(errorTransactionLog);
            });



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