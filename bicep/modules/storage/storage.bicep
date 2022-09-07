param location string
param suffix string

var strLogicApp = 'strl${suffix}'

resource storageAccountLogicApp 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: strLogicApp
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {    
    accessTier: 'Hot'
  }  
}

output strName string = storageAccountLogicApp.name
