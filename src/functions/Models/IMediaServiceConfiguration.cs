using System;

namespace Contoso
{
    public interface IMediaServiceConfiguration
    {
        string AadClientId { get; }
        Uri AadEndpoint { get; }
        string AadSecret { get; }
        string AadTenantId { get; }
        string AccountName { get; }
        Uri ArmAadAudience { get; }
        Uri ArmEndpoint { get; }
        string Location { get; }
        string ResourceGroup { get; }
        string SubscriptionId { get; }
        string StorageAccountName { get; }

        string UploadVideoStorageAccount { get; }

        string UploadVideoContainer { get; }
    }
}