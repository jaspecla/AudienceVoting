﻿@description('Base name for all resources')
param resourceBaseName string = 'audience-voting'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Azure Cosmos DB account name, max length 44 characters')
param accountName string = '${resourceBaseName}-cosmos'

@description('The primary region for the Azure Cosmos DB account.')
param primaryRegion string = location

@allowed([
  'Eventual'
  'ConsistentPrefix'
  'Session'
  'BoundedStaleness'
  'Strong'
])
@description('The default consistency level of the Cosmos DB account.')
param defaultConsistencyLevel string = 'Session'

@minValue(10)
@maxValue(2147483647)
@description('Max stale requests. Required for BoundedStaleness. Valid ranges, Single Region: 10 to 2147483647. Multi Region: 100000 to 2147483647.')
param maxStalenessPrefix int = 100000

@minValue(5)
@maxValue(86400)
@description('Max lag time (minutes). Required for BoundedStaleness. Valid ranges, Single Region: 5 to 84600. Multi Region: 300 to 86400.')
param maxIntervalInSeconds int = 300

@allowed([
  true
  false
])
@description('Enable system managed failover for regions')
param systemManagedFailover bool = true

@description('The name for the database')
param databaseName string = 'AudienceVoting'

@description('The name for the votes container')
param votesContainerName string = 'Votes'

@description('The name for the teams container')
param teamsContainerName string = 'Teams'

var containers = [
    {
        name: votesContainerName
        partitionKey: '/id'
    }
    {
        name: teamsContainerName
        partitionKey: '/id'
    }
]

var consistencyPolicy = {
  Eventual: {
    defaultConsistencyLevel: 'Eventual'
  }
  ConsistentPrefix: {
    defaultConsistencyLevel: 'ConsistentPrefix'
  }
  Session: {
    defaultConsistencyLevel: 'Session'
  }
  BoundedStaleness: {
    defaultConsistencyLevel: 'BoundedStaleness'
    maxStalenessPrefix: maxStalenessPrefix
    maxIntervalInSeconds: maxIntervalInSeconds
  }
  Strong: {
    defaultConsistencyLevel: 'Strong'
  }
}
var locations = [
  {
    locationName: primaryRegion
    failoverPriority: 0
    isZoneRedundant: false
  }
]

resource dbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower(accountName)
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: consistencyPolicy[defaultConsistencyLevel]
    locations: locations
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: systemManagedFailover
    capabilities: [
        {
            name: 'EnableServerless'
        }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: '${dbAccount.name}/${databaseName}'
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource dbContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = [for container in containers: {
  name: '${database.name}/${container.name}'
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [
          container.partitionKey
        ]
        kind: 'Hash'
      }
    }
  }
}]

param appServicePlanSku string = 'S1'

var appServicePlanName = '${resourceBaseName}-svcplan'
resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: appServicePlanSku
  }
  kind: 'linux'
}

param appPlatform string = 'DOTNETCORE|6.0' // The runtime stack of web app

var webAppName = '${resourceBaseName}-webapp'

resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: appPlatform
      appSettings: [
        {
          name: 'CosmosDbEndpoint'
          value: dbAccount.properties.documentEndpoint
        }
        {
          name: 'AuthType'
          value: 'Key'
        }
        {
          name: 'CosmosDbKey'
          value: listKeys(dbAccount.id, dbAccount.apiVersion).primaryMasterKey
        }
      ]
    }
  }
}

