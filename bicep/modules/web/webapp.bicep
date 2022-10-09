param location string
param suffix string
param appInsightName string

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightName
}

resource serverFarm 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: 'plan-${suffix}'
  location: location
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
    size: 'P1v2'
    family: 'P1v2'
    capacity: 1
  }
  kind: 'app'
}

// resource webApp 'Microsoft.Web/sites@2020-06-01' = {
//   name: 'frontend-${suffix}'
//   location: location  
//   properties: {
//     serverFarmId: serverFarm.id    
//     siteConfig: {
//       appSettings: [
//         {
//           name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
//           value: appInsights.properties.InstrumentationKey
//         }
//         {
//           name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
//           value: appInsights.properties.ConnectionString
//         }
//         {
//           name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
//           value: '~2'
//         }        
//       ]
//       vnetRouteAllEnabled: true
//       metadata: [
//         {
//           name: 'CURRENT_STACK'
//           value: 'dotnet'
//         }
//       ]
//       netFrameworkVersion: 'v6.0'
//       alwaysOn: true      
//     }    
//     clientAffinityEnabled: false
//     httpsOnly: true
//   }  
// }
