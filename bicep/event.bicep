param functionId string
param location string
param storageId string
param storageName string

// resource spokeRg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
//   name: spokeRgName  
// }

module event 'modules/eventGrid/systemTopics.bicep' = {
  name: 'event'
  params: {
    functionId: functionId
    location: location
    storageId: storageId
    storageName: storageName
  }
}
