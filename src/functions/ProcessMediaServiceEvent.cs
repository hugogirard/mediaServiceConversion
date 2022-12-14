// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using Contoso.Infrastructure.Models;
using System.Threading.Tasks;
using functions.Models;
using Contoso;

namespace functions
{
    public class ProcessMediaServiceEvent
    {
        private readonly IMediaService _mediaService;

        public ProcessMediaServiceEvent(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [FunctionName("ProcessMediaServiceEvent")]
        public async Task Run([EventGridTrigger]EventGridEvent eventGridEvent,
                               [CosmosDB(databaseName: "videos",
                                         collectionName: "mediaInsights",
                                         ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
                               [CosmosDB(databaseName: "videos",
                                         collectionName: "mediaInsights",
                                         ConnectionStringSetting = "CosmosDBConnection")]
                                         IAsyncCollector<VideoEncodingJob> videos,
                                ILogger log)
        {
            // Get job ID (primary key of CosmosDB)
            var jobId = eventGridEvent.Subject.Split('/').Last();
            var documentId = jobId.Replace("job-", "");
            
            log.LogDebug($"Receive event: {eventGridEvent.EventType}");
            log.LogDebug($"Job ID: {jobId}");
            log.LogDebug($"Document ID: {documentId}");

            log.LogDebug($"{eventGridEvent.Data.ToString()}");

            if (eventGridEvent.EventType == "Microsoft.Media.JobStateChange") 
            {
                try
                {
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("videos", "mediaInsights");

                    // Retrieve the document and update it
                    IDocumentQuery<VideoEncodingJob> query = client.CreateDocumentQuery<VideoEncodingJob>(collectionUri)
                                                                   .Where(p => p.Id == documentId)
                                                                   .AsDocumentQuery();

                    // This should return only one document
                    while (query.HasMoreResults)
                    {
                        log.LogDebug("Document found in CosmosDB");

                        foreach (VideoEncodingJob video in await query.ExecuteNextAsync())
                        {
                            log.LogDebug($"Get document: {video.Id}");

                            // Updating the video status
                            var jobStateEvent = JsonConvert.DeserializeObject<JobStateEvent>(eventGridEvent.Data.ToString());
                            video.State = jobStateEvent.State;

                            // Get Streaming Url if job finished successfully
                            if (jobStateEvent.State == JobState.Finished) 
                            {
                                video.StreamingLocationUrl = await _mediaService.GetStreamingUrlAsync(video.OutputAssetName);
                            }

                            await videos.AddAsync(video);

                            break;
                        }
                        
                    }                                                                         
                }
                catch (System.Exception ex)
                {
                    log.LogError(ex.Message,ex);
                    log.LogError(ex.StackTrace);
                }

            }
            
        }
    }
}
