using Azure.Core;
using Azure.Identity;
using Contoso;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso
{
    public class MediaServiceFactory : IMediaServiceFactory
    {
        private readonly IMediaServiceConfiguration _mediaServiceConfiguration;
        private readonly string TokenType = "Bearer";

        public MediaServiceFactory(IMediaServiceConfiguration mediaServiceConfiguration)
        {
            _mediaServiceConfiguration = mediaServiceConfiguration;
        }

        public async Task<IAzureMediaServicesClient> GetMediaServiceClientAsync()
        {
            ServiceClientCredentials credentials = await GetCredentialsAsync();

            return new AzureMediaServicesClient(_mediaServiceConfiguration.ArmEndpoint, credentials)
            {
                SubscriptionId = _mediaServiceConfiguration.SubscriptionId
            };
        }

        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            var scopes = new[] { _mediaServiceConfiguration.ArmAadAudience + "/.default" };
            string token;
            if (_mediaServiceConfiguration.AadClientId != null) // Service Principal
            {
                var app = ConfidentialClientApplicationBuilder.Create(_mediaServiceConfiguration.AadClientId)
                                                              .WithClientSecret(_mediaServiceConfiguration.AadSecret)
                                                              .WithAuthority(AzureCloudInstance.AzurePublic, _mediaServiceConfiguration.AadTenantId)
                                                              .Build();

                var authResult = await app.AcquireTokenForClient(scopes)
                                                        .ExecuteAsync()
                                                        .ConfigureAwait(false);

                token = authResult.AccessToken;
            }
            else // managed identity
            {
                var credential = new ManagedIdentityCredential();
                var accessTokenRequest = await credential.GetTokenAsync(
                    new TokenRequestContext(
                        scopes: scopes
                        )
                    );
                token = accessTokenRequest.Token;
            }
            return new TokenCredentials(token, TokenType);
        }
    }
}
