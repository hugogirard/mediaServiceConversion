targetScope = 'subscription'

param functionId string
param location string
param storageId string
param storageName string
param spokeRgName string
param mediaId string

resource spokeRg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: spokeRgName  
}

module event 'modules/eventGrid/systemTopics.bicep' = {
  name: 'event'
  scope: resourceGroup(spokeRg.name)
  params: {
    functionId: functionId
    location: location
    storageId: storageId
    storageName: storageName
    mediaId: mediaId
  }
}
