using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace BlinkingLights
{
    public static class DataSource
    {
        private static Random _random = new Random();
        private static HashSet<string> _targets = new HashSet<string>() { "LiveData", "LiveData1", "LiveData2", "LiveData3" };

        [FunctionName("DataSource")]
        public static async Task Run([TimerTrigger("* * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            foreach (var target in _targets)
            {
                var data = new
                {
                    time = DateTimeOffset.UtcNow,
                    value = DateTimeOffset.UtcNow.Second + (_random.NextDouble() * 30) - 15
                };
                await SignalRBroadcast(target, JsonConvert.SerializeObject(data)); 
            }
        }

        private static async Task SignalRBroadcast(string target, params string[] arguments)
        {
            var (endpoint, accessKey) = ConnectionStringHelpers.ParseSignalRConnectionString();
            var token = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(accessKey)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim("aud", $"{endpoint}:5002/api/v1-preview/hub/notifications")
                .Build();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"{endpoint}:5002")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await httpClient.PostAsJsonAsync("/api/v1-preview/hub/notifications", new
            {
                target,
                arguments
            });
        }
    }
}
