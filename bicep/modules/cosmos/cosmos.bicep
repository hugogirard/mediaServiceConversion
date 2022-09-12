param location string
param suffix string

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: 'cosmos-${suffix}'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
  }
}

resource cosmosSql 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: cosmos
  name: 'videos'
  properties: {
    resource: {
      id: 'videos'
    }
  }
}
