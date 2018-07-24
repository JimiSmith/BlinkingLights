using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;

namespace BlinkingLights
{
    public static class SignalR
    {
        [FunctionName("Negotiate")]
        public static IActionResult Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "{hub}/negotiate")] HttpRequest req,
            string hub,
            TraceWriter log)
        {
            var (endpoint, accessKey) = ConnectionStringHelpers.ParseSignalRConnectionString();
            var token = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(accessKey)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim("aud", $"{endpoint}:5001/client/?hub={hub}")
                .Build();
            return new OkObjectResult(new
            {
                url = $"{endpoint}:5001/client/?hub={hub}",
                accessToken = token,
                availableTransports = Enumerable.Empty<string>()
            });
        }
    }
}
