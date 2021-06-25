using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Playwright;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace HGV.Quarterstaff
{
    public class ImageCaptureFunction
    {
        private readonly BlobContainerClient container;

        public ImageCaptureFunction(BlobContainerClient container)
        {
            this.container = container;
        }

        [Function("Summary")]
        public async Task<HttpResponseData> Summary(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "match/{id}/summary")] HttpRequestData req, long id,
            FunctionContext executionContext
        )
        {
            var client = this.container.GetBlobClient($"summary/{id}.png");
            var result = await client.ExistsAsync();
            if(result)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "image/png");
                await client.DownloadToAsync(response.Body);
                return response;
            } 
            else
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
                var page = await browser.NewPageAsync();
                await page.GotoAsync($"https://abilitydraft.datdota.com/matches/{id}"); // 6049862741

                var element = await page.QuerySelectorAsync(".match_summary");
                var data = await element.ScreenshotAsync(new() { Type = ScreenshotType.Png });
                await element.DisposeAsync();

                await client.UploadAsync(new BinaryData(data));

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "image/png");
                await response.WriteBytesAsync(data);

                return response;
            }
        }
    }
}
