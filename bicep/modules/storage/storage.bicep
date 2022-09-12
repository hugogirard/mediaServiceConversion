param location string
param suffix string

//var strLogicApp = 'strl${suffix}'
var strFunctionApp = 'strf${suffix}'
var strMedia = 'strm${suffix}'

// resource storageAccountLogicApp 'Microsoft.Storage/storageAccounts@2021-04-01' = {
//   name: strLogicApp
//   location: location
//   tags: {
//     description: 'logic app storage'
//   }
//   sku: {
//     name: 'Standard_LRS'
//   }
//   kind: 'StorageV2'
//   properties: {    
//     accessTier: 'Hot'
//   }  
// }

resource storageFunction 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: strFunctionApp
  location: location
  tags: {
    description: 'function service storage'
  }
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {    
    accessTier: 'Hot'
  }  
}

resource storageMedia 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: strMedia
  location: location
  tags: {
    description: 'media service storage'
  }
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {    
    accessTier: 'Hot'
  }  
}

resource containerVideo 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
  name: '${storageMedia.name}/default/videos'  
}

output strFunctionAppName string = storageFunction.name
output strMediaId string = storageMedia.id
output strMediaName string = storageMedia.name
