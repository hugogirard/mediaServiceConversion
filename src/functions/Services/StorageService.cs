using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Contoso;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace functions.Services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public StorageService(IMediaServiceConfiguration config)
        {

            var blobClient = new BlobServiceClient(config.UploadVideoStorageAccount);
            _containerClient = blobClient.GetBlobContainerClient(config.UploadVideoContainer);

        }

        public Uri GetSharedAccessReferenceForUpload(string filename)
        {
            var blobClient = _containerClient.GetBlobClient(filename);

            // In production use User Delegation SAS
            // https://learn.microsoft.com/en-us/azure/storage/common/storage-sas-overview#user-delegation-sas
            return blobClient.GenerateSasUri(BlobSasPermissions.Write,
                                             DateTime.UtcNow.AddMinutes(30));

        }
    }
}
