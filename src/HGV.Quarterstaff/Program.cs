using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace HGV.Quarterstaff
{
    public class Program
    {
        readonly static string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        public static async Task Main(string[] args)
        {
            var container = new BlobContainerClient(_storageConnectionString, "quarterstaff");
            await container.CreateIfNotExistsAsync();

            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services => {
                	services.AddLogging();
                    services.AddSingleton(container);
                })
                .Build();

            await host.RunAsync();
        }
    }
}