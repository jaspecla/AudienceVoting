using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{

  public class CosmosDbContainerService
  {
    private string _databaseName = "AudienceVoting";
    private CosmosClient _client;
    private Database? _database;
    private int _defaultThroughput = 400;
    private bool _didInitializeDb = false;

    private Dictionary<string, Container> _containers;

    public CosmosDbContainerService(IConfiguration configuration)
    {
      var cosmosDbOptions = new CosmosClientOptions
      {
        SerializerOptions = new CosmosSerializationOptions
        {
          PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
      };

      _client = new(
        accountEndpoint: configuration["CosmosDbEndpoint"],
        authKeyOrResourceToken: configuration["CosmosDbKey"],
        clientOptions: cosmosDbOptions
      );

      _containers = new Dictionary<string, Container>();
    }

    private async Task<Database> GetDatabaseOrCreateAsync()
    {
      DatabaseResponse response = await _client.CreateDatabaseIfNotExistsAsync(id: _databaseName);
      return response.Database;
    }

    public async Task<Container> GetContainerOrCreateAsync(string containerName)
    {
      if (_containers.ContainsKey(containerName))
      {
        return _containers[containerName];
      }

      if (_database == null)
      {
        _database = await GetDatabaseOrCreateAsync();
      }

      ContainerResponse response = await _database.CreateContainerIfNotExistsAsync(id: containerName, partitionKeyPath: "/id", throughput: _defaultThroughput);

      _containers.Add(containerName, response.Container);

      return response.Container;
    }

  }
}
