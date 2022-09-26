param location string
param suffix string
param strAccountName string
param strMediaName string
param appInsightName string
param cosmosDbName string
param mediaServiceName string

@secure()
param aadClientId string

@secure()
param aadClientSecret string

var appServiceName = 'plan-function-${suffix}'

resource insight 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightName
}

resource storage 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  name: strAccountName 
}

resource storageMedia 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  name: strMediaName 
}

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' existing = {
  name: cosmosDbName
}

resource serverFarm 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServiceName
  location: location
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
  kind: 'elastic'
  properties: {
    maximumElasticWorkerCount: 100    
  }
}

var functionAppName = 'fnc-${suffix}'

resource function 'Microsoft.Web/sites@2020-06-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: serverFarm.id
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: insight.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: insight.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${strAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${strAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~12'
        }
        {
          name: 'StrMediaCnxString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageMedia.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageMedia.id, storageMedia.apiVersion).keys[0].value}'
        }
        {
          name: 'CosmosDBConnection'
          value: '${cosmos.listConnectionStrings().connectionStrings[0]}'
        }
        {
          name: 'SubscriptionId'
          value: subscription().subscriptionId
        }
        {
          name: 'ResourceGroup'
          value: resourceGroup().name
        }
        {
          name: 'AccountName'
          value: mediaServiceName
        }
        {
          name: 'AadTenantId'
          value: subscription().tenantId
        }
        {
          name: 'AadClientId'
          value: aadClientId
        }
        {
          name: 'AadSecret'
          value: aadClientSecret
        }
        {
          name: 'ArmAadAudience'
          value: 'https://management.core.windows.net/'
        }
        {
          name: 'AadEndpoint'
          value: 'https://management.azure.com/'
        }
        {
          name: 'Location'
          value: resourceGroup().location
        }
        {
          name: 'StorageAccountName'
          value: strMediaName
        }
      ]
    }
  }
}

output functionName string = function.name
output functionId string = function.id
