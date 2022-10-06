using Contoso.Infrastructure.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso
{
    public class MediaService : IMediaService
    {
        private IMediaServiceFactory _mediaServiceFactory;
        private readonly IMediaServiceConfiguration _configuration;
        private readonly TelemetryClient _telemetryClient;
        private IAzureMediaServicesClient _azureMediaServicesClient;

        private readonly string TRANSFORM_NAME = "DefaultTransform";
        private readonly string BUILT_IN_PRESET = "AdaptiveStreaming";
        private readonly string DEFAULT_STREAMING_ENDPOINT = "default";

        public MediaService(IMediaServiceFactory mediaServiceFactory, 
                            IMediaServiceConfiguration configuration,
                            TelemetryClient telemetryClient)
        {
            _mediaServiceFactory = mediaServiceFactory;
            _configuration = configuration;
            _telemetryClient = telemetryClient;
        }

        public async Task<VideoEncodingJob> SubmitJobAsync(Uri videoUrl,
                                                           string videoName,
                                                           string videoDescription,
                                                           string fileName)
        {

            _azureMediaServicesClient = await _mediaServiceFactory.GetMediaServiceClientAsync();

            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            string jobName = $"job-{uniqueness}";
            string outputAssetName = $"output-{uniqueness}";



            var videoEncodingJob = new VideoEncodingJob(fileName, videoName, videoDescription)
            {
                Id = uniqueness,
                JobName = jobName,
                OutputAssetName = outputAssetName
            };
            
            try
            {
                Transform transform = await CreateTransformAsync();
                Asset outputAsset = await CreateOutputAssetAsync(outputAssetName);

                Job job = await SubmitJobAsync(videoUrl.ToString(), jobName, outputAssetName);
                videoEncodingJob.StartedTime = job.StartTime;
                videoEncodingJob.State = job.State;

            }
            catch
            {
                throw;
            }

            return videoEncodingJob;

        }

        public async Task<bool> AssetExistAsync(string assetName)
        {
            _azureMediaServicesClient = await _mediaServiceFactory.GetMediaServiceClientAsync();

            try
            {
                await _azureMediaServicesClient.Assets.GetAsync(_configuration.ResourceGroup,
                                                _configuration.AccountName,
                                                assetName);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return true;
        }

        public async Task<IList<string>> GetStreamingUrlAsync(string assetName)
        {

            _azureMediaServicesClient = await _mediaServiceFactory.GetMediaServiceClientAsync();


            try
            {
                var locatorName = await CreateStreamingLocatorAsync(assetName);
                return await GetStreamingEndpointAsync(assetName, locatorName);
            }
            catch (Exception ex)
            {

                
            }

            return null;
        }

        private async Task<IList<string>> GetStreamingEndpointAsync(string assetName, string locatorName)
        {

            IList<string> streamingUrls = new List<string>();

            var streamingEndpoint = await _azureMediaServicesClient.StreamingEndpoints.GetAsync(_configuration.ResourceGroup,
                                                                                                _configuration.AccountName,
                                                                                                DEFAULT_STREAMING_ENDPOINT);

            // Start Streaming Endpoint
            if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running) 
            {
                await _azureMediaServicesClient.StreamingEndpoints.StartAsync(_configuration.ResourceGroup,
                                                                              _configuration.AccountName,
                                                                              DEFAULT_STREAMING_ENDPOINT);
            }

            ListPathsResponse paths = await _azureMediaServicesClient.StreamingLocators.ListPathsAsync(_configuration.ResourceGroup,
                                                                              _configuration.AccountName,
                                                                              locatorName);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName,

                    Path = path.Paths[0]
                };
                streamingUrls.Add(uriBuilder.ToString());
            }
            return streamingUrls;
        }

        private async Task<string> CreateStreamingLocatorAsync(string assetName) 
        {
            string locatorName = $"locator_{assetName}";

            StreamingLocator locator = await _azureMediaServicesClient
                                                    .StreamingLocators
                                                    .CreateAsync(_configuration.ResourceGroup,
                                                                 _configuration.AccountName,
                                                                 locatorName,
                                                                 new StreamingLocator
                                                                 {
                                                                     AssetName = assetName,
                                                                     StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                                                                 });
            return locatorName;
        }

        private async Task<Job> SubmitJobAsync(string videoUrl, string jobName, string outputAssetName)
        {
            var jobInput = new JobInputHttp(files: new[] { videoUrl });

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };


            return await _azureMediaServicesClient.Jobs.CreateAsync(_configuration.ResourceGroup,
                                                                    _configuration.AccountName,
                                                                    TRANSFORM_NAME,
                                                                    jobName,
                                                                    new Job
                                                                    {
                                                                        Input = jobInput,
                                                                        Outputs = jobOutputs
                                                                    });
        }

        private async Task<Asset> CreateOutputAssetAsync(string assetName)
        {
            Asset asset;
            try
            {
                // Check if Asset already exists
                asset = await _azureMediaServicesClient.Assets.GetAsync(_configuration.ResourceGroup,
                                                                        _configuration.AccountName,
                                                                        assetName);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                asset = new Asset(storageAccountName: _configuration.StorageAccountName);
            }

            return await _azureMediaServicesClient.Assets.CreateOrUpdateAsync(_configuration.ResourceGroup,
                                                                              _configuration.AccountName,
                                                                              assetName,
                                                                              asset);
        }

        private async Task<IAzureMediaServicesClient> GetAzureMediaServicesClientAsync()
        {
            if (_azureMediaServicesClient == null)
                return await _mediaServiceFactory.GetMediaServiceClientAsync();

            return _azureMediaServicesClient;
        }

        private async Task<Transform> CreateTransformAsync()
        {
            bool createTransform = false;
            Transform transform = null;

            // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            try
            {
                transform = await _azureMediaServicesClient.Transforms.GetAsync(_configuration.ResourceGroup,
                                                                                _configuration.AccountName,
                                                                                TRANSFORM_NAME);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                createTransform = true;
            }

            if (createTransform)
            {
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                  new TransformOutput(
                        new BuiltInStandardEncoderPreset()
                        {
                            // Pass the buildin preset name.
                            PresetName = BUILT_IN_PRESET
                        },
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = $"An encoding transform using {BUILT_IN_PRESET} preset";

                // Create the Transform with the outputs defined above
                transform = await _azureMediaServicesClient.Transforms.CreateOrUpdateAsync(_configuration.ResourceGroup,
                                                                                           _configuration.AccountName,
                                                                                           TRANSFORM_NAME,
                                                                                           outputs,
                                                                                           description);
            }
            else
            {
                //log.LogInformation($"Transform '{transformName}' found in AMS account.");
            }

            return transform;
        }
    }
}
