param location string
param storageName string
param functionId string
param storageId string
param mediaId string

var topicName = '${storageName}-${guid(subscription().subscriptionId)}'

resource systemTopicStorage 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  name: topicName
  location: location
  properties: {
    source: storageId
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

resource systemTopicMedia 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  name: topicName
  location: location
  properties: {
    source: mediaId
    topicType: 'Microsoft.Media.MediaServices'
  }
}

resource eventSubsStorage 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2020-10-15-preview' = {
  name: '${topicName}/ToAzureFuncSubsStorage'
  dependsOn: [
    systemTopicStorage
  ]
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

resource eventSubsMedia 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2020-10-15-preview' = {
  name: '${topicName}/ToAzureFuncSubsMedia'
  dependsOn: [
    systemTopicStorage
  ]
  properties: {
    destination: {
      properties: {
        resourceId: '${functionId}/functions/ProcessMediaServiceEvent'
      }
      endpointType: 'AzureFunction'
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Media.JobStateChange'      
      ]
      enableAdvancedFilteringOnArrays: false
    }
    eventDeliverySchema: 'EventGridSchema'    
  }
}
