using System;
using System.Data.Common;

namespace BlinkingLights
{
    public static class ConnectionStringHelpers
    {
        public static (string endpoint, string accessKey) ParseSignalRConnectionString()
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = Environment.GetEnvironmentVariable("Azure:SignalR:ConnectionString")
            };
            return (builder["Endpoint"].ToString(), builder["AccessKey"].ToString());
        }
    }
}
