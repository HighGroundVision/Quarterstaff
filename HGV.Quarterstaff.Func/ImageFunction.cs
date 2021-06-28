using Azure.Storage.Blobs;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HGV.Quarterstaff.Func
{
    public class ImageFunction
    { 
        [FunctionName(nameof(GetImageSummary))]
        //[OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public IActionResult GetImageSummary(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/{id}/summary")] HttpRequest req, long id,
            [Blob("quarterstaff/{id}/summary.png", FileAccess.Read, Connection = "AzureWebJobsStorage")]byte[] data,
            ILogger log)
        {
            if(data is null)
                return new NotFoundResult();
            else 
                return new FileContentResult(data, "image/png");
        }

        [FunctionName(nameof(GetImagePlayers))]
        public IActionResult GetImagePlayers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/{id}/players")] HttpRequest req, long id,
            [Blob("quarterstaff/{id}/players.png", FileAccess.Read, Connection = "AzureWebJobsStorage")]byte[] data,
            ILogger log)
        {
            if(data is null)
                return new NotFoundResult();
            else 
                return new FileContentResult(data, "image/png");
        }

        [FunctionName(nameof(GetImageDraft))]
        public IActionResult GetImageDraft(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/{id}/draft")] HttpRequest req, long id,
            [Blob("quarterstaff/{id}/draft.gif", FileAccess.Read, Connection = "AzureWebJobsStorage")]byte[] data,
            ILogger log)
        {
            if(data is null)
                return new NotFoundResult();
            else 
                return new FileContentResult(data, "image/gif");
        }
    }
}

