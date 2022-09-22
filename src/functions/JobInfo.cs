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
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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
                                                              PartitionKey = "{Query.id}")] VideoEncodingJob videoEncodingJob,
                                                    [CosmosDB(databaseName: "videos",
                                                              collectionName: "mediaInsights",
                                                              ConnectionStringSetting = "CosmosDBConnection")]
                                                              IAsyncCollector<VideoEncodingJob> videos) 
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
                    if (videoEncodingJob.StreamingLocationUrl == null || videoEncodingJob.StreamingLocationUrl.Count() == 0) 
                    {
                        videoEncodingJob.StreamingLocationUrl = await _mediaService.GetStreamingUrlAsync(videoEncodingJob.OutputAssetName);
                        await videos.AddAsync(videoEncodingJob);
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

        // This should not be done in production if you have a lot of documents to return
        // you should return a subset and use pagination
        [FunctionName("GetJobs")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "GetJobs" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]        
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<VideoEncodingJob>), Description = "The lists of Jobs")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
                                             [CosmosDB(databaseName: "videos",
                                                       collectionName: "mediaInsights",
                                                       ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client)
        {

            var videoEncodingJobs = new List<VideoEncodingJob>();

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("videos", "mediaInsights");

            IDocumentQuery<VideoEncodingJob> query = client.CreateDocumentQuery<VideoEncodingJob>(collectionUri).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (VideoEncodingJob result in await query.ExecuteNextAsync())
                {
                    videoEncodingJobs.Add(result);
                }
            }

            return new OkObjectResult(videoEncodingJobs);
        }
    }
}

