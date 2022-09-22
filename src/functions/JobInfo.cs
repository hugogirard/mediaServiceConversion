using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contoso;
using Contoso.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace functions
{
    public class JobInfo
    {
        private readonly ILogger<JobInfo> _logger;
        private readonly IMediaService _mediaService;

        public JobInfo(ILogger<JobInfo> log, IMediaService mediaService)
        {
            _logger = log;
            _mediaService = mediaService;
        }

        [FunctionName("GetJobById")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "GetJobById" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The ID of the video")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VideoEncodingJob), Description = "The OK response")]
        public async Task<IActionResult> GetJobById([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
                                                    [CosmosDB(databaseName: "videos",
                                                              collectionName: "mediaInsights",
                                                              ConnectionStringSetting = "CosmosDBConnection",                                                                                              
                                                              Id = "{Query.id}",
                                                              PartitionKey = "{Query.id}")] VideoEncodingJob videoEncodingJob) 
        {
            try
            {
                if (videoEncodingJob == null)
                    return new NotFoundObjectResult($"The VideoEncoding job for ID: {req.Query["id"]} cannot be found");

                // Validate if the asset is present
                if (!await _mediaService.AssetExistAsync(videoEncodingJob.OutputAssetName))
                {
                    return new NotFoundObjectResult($"The asset {videoEncodingJob.OutputAssetName} for ID: {videoEncodingJob.Id} is not found");
                }

                // Get StreamingUrl if job finished processing
                if (videoEncodingJob.State == JobState.Finished)
                { 
                    if (string.IsNullOrEmpty(videoEncodingJob.StreamingLocationUrl)) 
                    { 
                        
                    }
                }

                return new OkObjectResult(videoEncodingJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(500);                
            }
        }

        //[FunctionName("GetJobs")]
        //[OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        //public async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        //{
        //    _logger.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}
    }
}

