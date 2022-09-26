param location string
param storageName string
param functionId string
param storageId string

var topicName = '${storageName}-${guid(subscription().subscriptionId)}'

resource systemTopic 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  name: topicName
  location: location
  properties: {
    source: storageId
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

resource eventSubs 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2020-10-15-preview' = {
  name: '${topicName}/ToAzureFuncSubs'
  dependsOn: [
    systemTopic
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
