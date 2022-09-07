targetScope = 'subscription'

param location string

var spokeRgName = 'rg-spoke-media'

var spokeConversionSuffix = uniqueString(spokeRg.id)

resource spokeRg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: spokeRgName
  location: location
}

module monitoring 'modules/monitoring/monitoring.bicep' = {
  scope: resourceGroup(spokeRg.name)
  name: 'monitoring'
  params: {
    location: location
    suffix: spokeConversionSuffix
  }
}

module storage 'modules/storage/storage.bicep' = {
  scope: resourceGroup(spokeRg.name)
  name: 'storage'
  params: {
    location: location
    suffix: spokeConversionSuffix
  }
}

module logicApp 'modules/logic/logicapp.bicep' = {
  scope: resourceGroup(spokeRg.name)
  name: ''
  params: {
    appInsightName: monitoring.outputs.insightName
    location: location
    storageName: storage.outputs.strName
    suffix: spokeConversionSuffix
  }
}
