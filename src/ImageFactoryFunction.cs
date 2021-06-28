using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ImageMagick;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace HGV.Quarterstaff.Func
{
    public class ImageFactoryFunction
    {
        private readonly ConnectOptions puppeterOptions;
        private readonly ViewPortOptions viewportOptions;
        private readonly ScreenshotOptions screenshotOptions;
        private readonly BlobContainerClient container;

        public ImageFactoryFunction()
        {
            var cs = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            this.container = new BlobContainerClient(cs, "quarterstaff");
            this.container.CreateIfNotExists();

            var token = Environment.GetEnvironmentVariable("BrowserlessToken");
            this.puppeterOptions = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };
            this.viewportOptions = new ViewPortOptions { Width = 1920, Height = 1080 };
            this.screenshotOptions = new ScreenshotOptions() { Type = ScreenshotType.Png };
        }

        [FunctionName(nameof(ImageFactoryStart))]
        public async Task<HttpResponseMessage> ImageFactoryStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "generate/{id}")] HttpRequestMessage req, long id,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = $"{Guid.NewGuid()}";
            await starter.StartNewAsync(nameof(ImageFactoryOrchestrator), instanceId, id);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(ImageFactoryOrchestrator))]
        public async Task ImageFactoryOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext cxt, ILogger log)
        {
            // var logger = cxt.CreateReplaySafeLogger(log);
            var id = cxt.GetInput<long>();

            cxt.SetCustomStatus("Generating Snapshots");

            var options = new RetryOptions(TimeSpan.FromSeconds(3), 3);
            await cxt.CallActivityWithRetryAsync(nameof(ImageFactoryGenerateSnapshots), options, id);

            cxt.SetCustomStatus("Generating Steps");

            var tasks = new List<Task>();
            for (int i = 0; i <= 40; i++)
            {
                tasks.Add(cxt.CallActivityWithRetryAsync(nameof(ImageFactorGenerateDraftStep), options, (id, i)));
            }
            await Task.WhenAll(tasks);

            cxt.SetCustomStatus("Generating Draft");

            await cxt.CallActivityWithRetryAsync(nameof(ImageFactoryGenerateDraft), options, id);
        }

        [FunctionName(nameof(ImageFactoryGenerateSnapshots))]
        public async Task ImageFactoryGenerateSnapshots([ActivityTrigger] IDurableActivityContext cxt, ILogger log)
        {
            var id = cxt.GetInput<long>();

            var browser = await Puppeteer.ConnectAsync(this.puppeterOptions);
            try
            {
                var page = await browser.NewPageAsync();
                await page.SetViewportAsync(this.viewportOptions);
                await page.GoToAsync($"http://ad.datdota.com/matches/{id}"); // 6049862741

                {
                    var data = await GetScreenshot(page, ".match_summary");
                    var client = this.container.GetBlobClient($"/{id}/summary.png");
                    await client.UploadAsync(data, true);
                }
                {
                    var data = await GetScreenshot(page, ".match_players");
                    var client = this.container.GetBlobClient($"/{id}/players.png");
                    await client.UploadAsync(data, true);
                }
           
                await page.DisposeAsync();
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        [FunctionName(nameof(ImageFactorGenerateDraftStep))]
        public async Task ImageFactorGenerateDraftStep([ActivityTrigger] IDurableActivityContext cxt, ILogger log)
        {
            (long id, int step) input = cxt.GetInput<(long,int)>();

            var browser = await Puppeteer.ConnectAsync(this.puppeterOptions);
            try
            {
                var page = await browser.NewPageAsync();
                await page.SetViewportAsync(this.viewportOptions);
                await page.GoToAsync($"http://ad.datdota.com/matches/{input.id}?step={input.step}");
                var data = await GetScreenshot(page, ".draft_replay_body");

                var i = input.step.ToString().PadLeft(2, '0');
                var client = this.container.GetBlobClient($"/{input.id}/step.{i}.png");
                await client.UploadAsync(data, true);

                await page.DisposeAsync();
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        [FunctionName(nameof(ImageFactoryGenerateDraft))]
        public async Task ImageFactoryGenerateDraft([ActivityTrigger] IDurableActivityContext cxt, ILogger log)
        {
            var id = cxt.GetInput<long>();

            using var collection = new MagickImageCollection();
            for (int step = 0; step <= 40; step++)
            {
                var image = await GetImage(id, step);
                collection.Add(image);
            }

            collection.OptimizePlus();

            var stream = new MemoryStream();
            await collection.WriteAsync(stream, MagickFormat.Gif);
            stream.Seek(0, SeekOrigin.Begin);
            var client = this.container.GetBlobClient($"/{id}/draft.gif");

            await client.UploadAsync(stream, true);
        }

        private async Task<MagickImage> GetImage(long id, int step)
        {
            var i = step.ToString().PadLeft(2, '0');
            var client = this.container.GetBlobClient($"/{id}/step.{i}.png");
            var stream = new MemoryStream();
            await client.DownloadToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var image = new MagickImage(stream, MagickFormat.Png);
            image.AnimationDelay = 25;
            return image;
        }

        private async Task<MemoryStream> GetScreenshot(Page page, string selector)
        {
            await page.WaitForSelectorAsync(selector);
            var element = await page.QuerySelectorAsync(selector);
            var data = await element.ScreenshotDataAsync(this.screenshotOptions);
            await element.DisposeAsync();
            var stream = new MemoryStream(data);
            return stream;
        }
        
        private async Task<MemoryStream> ConvertScreenshot(MemoryStream input)
        {
            using var collection = new MagickImageCollection();
            var image = new MagickImage(input.ToArray(), MagickFormat.Png);
            image.AnimationDelay = 25;
            collection.Add(image);

            var stream = new MemoryStream();
            await collection.WriteAsync(stream, MagickFormat.Gif);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}