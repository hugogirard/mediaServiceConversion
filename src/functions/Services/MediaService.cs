using Contoso.Infrastructure.Models;
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
        private IAzureMediaServicesClient _azureMediaServicesClient;

        private readonly string TRANSFORM_NAME = "DefaultTransform";
        private readonly string BUILT_IN_PRESET = "AdaptiveStreaming";


        public MediaService(IMediaServiceFactory mediaServiceFactory, IMediaServiceConfiguration configuration)
        {
            _mediaServiceFactory = mediaServiceFactory;
            _configuration = configuration;
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

                var jobInput = new JobInputHttp(files: new[] { videoUrl.ToString() });


            }
            catch
            {
                throw;
            }

            return videoEncodingJob;

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
