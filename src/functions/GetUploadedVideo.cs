// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using System.IO;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System.Linq;
using Contoso.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contoso
{
    public class GetUploadedVideo
    {
        private readonly IMediaService _mediaService;

        public GetUploadedVideo(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [FunctionName("GetUploadedVideo")]
        public async Task Run([EventGridTrigger]EventGridEvent eventGridEvent,
                              [Blob("{data.url}", FileAccess.Read, Connection = "StrMediaCnxString")] BlobClient blobClient,
                              [CosmosDB(databaseName: "videos",
                                        collectionName: "mediaInsights",
                                        ConnectionStringSetting = "CosmosDBConnection")]
                                        IAsyncCollector<VideoEncodingJob> videos,
                              ILogger log)
        {

            log.LogDebug($"Received event from storage: {eventGridEvent.EventType}");
            log.LogDebug($"Data of the event: {eventGridEvent.Data.ToString()}");

            // Validate the event type
            if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                log.LogDebug("Receveid event Microsoft.Storage.BlobCreated");

                // Retrieve metadata of the video
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                string videoName = properties.Metadata.FirstOrDefault(d => d.Key == "name").Value;
                string videoDescription = properties.Metadata.FirstOrDefault(d => d.Key == "description").Value;

                log.LogDebug($"Video name: {videoName}");
                log.LogDebug($"Video description: {videoDescription}");

                var sasVideo = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read,
                                                         DateTime.UtcNow.AddDays(1));

                log.LogDebug($"Sas Key: {sasVideo}");

                try
                {
                    
                    log.LogDebug("Creating encoding job video");

                    VideoEncodingJob videoJobEncoding = await _mediaService.SubmitJobAsync(sasVideo, videoName, videoDescription, blobClient.Name);

                    log.LogDebug($"Video job encoding started: {JsonConvert.SerializeObject(videoJobEncoding)}");

                    await videos.AddAsync(videoJobEncoding);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message, ex);
                }

            }                 
        }
    }
}
