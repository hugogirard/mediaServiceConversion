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

namespace functions
{
    public class ProcessMediaServiceEvent
    {
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
            
            log.LogDebug($"Receive event: {eventGridEvent.EventType}");
            log.LogDebug($"Job ID: ${jobId}");

            if (eventGridEvent.EventType == "Microsoft.Media.JobStateChange") 
            {
                try
                {
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("videos", "mediaInsights");

                    // Retrieve the document and update it
                    IDocumentQuery<VideoEncodingJob> query = client.CreateDocumentQuery<VideoEncodingJob>(collectionUri)
                                                                .Where(p => p.Id == jobId)
                                                                .AsDocumentQuery();

                    // This should return only one document
                    while (query.HasMoreResults)
                    {
                        log.LogDebug("Document found in CosmosDB");

                        foreach (VideoEncodingJob video in await query.ExecuteNextAsync())
                        {
                            // Updating the video status
                            dynamic data = JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

                            if (!string.IsNullOrEmpty(data?.state))
                            {
                                if (Enum.TryParse(data.state, out JobState newState))
                                {
                                    log.LogDebug($"Document new state: {newState}");
                                    video.State = newState;
                                }
                                else 
                                {
                                    video.Error = $"Cannot parse state in event: {data}";
                                    log.LogError($"Cannot update state for document :{jobId}");
                                }
                            }
                            else 
                            {
                                video.Error = $"Not state in event: {data}";
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
