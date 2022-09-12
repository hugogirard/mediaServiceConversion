param location string
param storageName string
param functionId string
param storageId string

var topicName = '${storageName}-${guid(subscription().subscriptionId)}'

var functionMethodName = 'GetUploadedVideo'

resource systemTopic 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  name: topicName
  location: location
  properties: {
    source: storageId
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

// resource eventSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2021-12-01' = {
//   parent: systemTopic
//   name: 'ToAzureFuncSubs'
//   properties: {
//     destination: {
//       properties: {
//         resourceId: '${functionId}/functions/${functionMethodName}'
//       }
//       endpointType: 'AzureFunction'
//     }
//     filter: {
//       includedEventTypes: [
//         'Microsoft.Storage.BlobCreated'
//       ]
//     }
//   }
// }
