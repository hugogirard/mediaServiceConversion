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

namespace Contoso
{
    public static class GetUploadedVideo
    {
        [FunctionName("GetUploadedVideo")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent,                    
                                     ILogger log)
        {
            //                   [Blob("{data.url}",FileAccess.Read,Connection = "StrCnx")] BlobClient blobClient,
            var stringify = JsonConvert.SerializeObject(eventGridEvent);
            log.LogInformation(stringify);
            //BlobProperties properties = await blobClient.GetPropertiesAsync();            
        }
    }
}
