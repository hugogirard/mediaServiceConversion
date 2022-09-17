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
        private readonly IMediaServiceFactory _mediaServiceFactory;

        public GetUploadedVideo(IMediaServiceFactory mediaServiceFactory)
        {
            _mediaServiceFactory = mediaServiceFactory;
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
            // Validate the event type
            if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                string[] paths = eventGridEvent.Subject.Split('/');
                string filename = paths.Last();

                var sasVideo = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read,
                                                         DateTime.UtcNow.AddDays(1));

                log.LogDebug(eventGridEvent.Data.ToString());
                //log.LogInformation(sasVideo.ToString());

                var videoJobEncoding = new VideoEncodingJob()
                {
                    StartedTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    FileMetadata = new VideoMetadata()
                    {
                        Name = filename
                    }
                };

                log.LogInformation(JsonConvert.SerializeObject(videoJobEncoding));

                try
                {
                    var client = await _mediaServiceFactory.GetMediaServiceClientAsync();
                    await videos.AddAsync(videoJobEncoding);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message, ex);
                }

            }

            //BlobProperties properties = await blobClient.GetPropertiesAsync();            
        }
    }
}
