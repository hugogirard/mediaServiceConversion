param location string
param storageName string
param functionId string
param storageId string
param mediaId string

var topicNameStorage = '${storageName}-${guid(subscription().subscriptionId)}'
var topicNameMedia = 'media-${guid(subscription().subscriptionId)}'

resource systemTopicStorage 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  name: topicNameStorage
  location: location
  properties: {
    source: storageId
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

// resource systemTopicMedia 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
//   name: topicNameMedia
//   location: location
//   properties: {
//     source: mediaId
//     topicType: 'Microsoft.Media.MediaServices'
//   }
// }

resource eventSubsStorage 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2020-10-15-preview' = {
  parent: systemTopicStorage
  name: 'ToAzureFuncSubsStorage'  
  properties: {
    destination: {
      properties: {
        resourceId: '${functionId}/functions/GetUploadedVideo'
      }
      endpointType: 'AzureFunction'
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'      
      ]
      enableAdvancedFilteringOnArrays: true
      advancedFilters: [
        {
          values: [
            'containers/videos/'            
          ]
          operatorType: 'StringContains'
          key: 'Subject'
        }        
      ]
    }
    eventDeliverySchema: 'EventGridSchema'    
  }
}

// resource eventSubsMedia 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2020-10-15-preview' = {
//   name: '${topicNameMedia}/ToAzureFuncSubsMedia'
//   dependsOn: [
//     systemTopicMedia
//   ]
//   properties: {
//     destination: {
//       properties: {
//         resourceId: '${functionId}/functions/ProcessMediaServiceEvent'
//       }
//       endpointType: 'AzureFunction'
//     }
//     filter: {
//       includedEventTypes: [
//         'Microsoft.Media.JobStateChange'      
//       ]
//       enableAdvancedFilteringOnArrays: false
//     }
//     eventDeliverySchema: 'EventGridSchema'    
//   }
// }
