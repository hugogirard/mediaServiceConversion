using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System.IO;

namespace UploaderConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)     
                            .AddUserSecrets<Program>()
                            .Build();

            var blobServiceClient = new BlobServiceClient(config["StrCnxString"]);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("videos");

            string localFilePath = config["FilePath"];
            string fileName = Path.GetFileName(localFilePath);

            IDictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("name", fileName);
            metadata.Add("description", "test video");

            BlobClient blob = container.GetBlobClient(fileName);

            using (var fs = File.OpenRead(localFilePath))
            {
                await blob.UploadAsync(fs, true);
            }

            await blob.SetMetadataAsync(metadata);
        }
    }
}