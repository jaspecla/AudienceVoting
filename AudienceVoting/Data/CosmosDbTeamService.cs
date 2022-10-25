using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{
  public class CosmosDbTeamService : ITeamService
  {
    private CosmosClient _cosmosDbClient;

    private string _databaseName = "AudienceVoting";
    private string _teamsContainerName = "Teams";
    private string _votesContainerName = "Votes";

    private Database? _database;
    private Container? _teamsContainer;
    private Container? _votesContainer;
    private int _defaultThroughput = 400;

    private bool _didInitializeDb = false;

    public CosmosDbTeamService(IConfiguration configuration)
    {
      var credential = new DefaultAzureCredential();

      var cosmosDbOptions = new CosmosClientOptions
      {
        SerializerOptions = new CosmosSerializationOptions
        {
          PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
      };

      if (configuration["AuthType"] == "Key")
      {
        _cosmosDbClient = new(
          accountEndpoint: configuration["CosmosDbEndpoint"],
          authKeyOrResourceToken: configuration["CosmosDbKey"],
          clientOptions: cosmosDbOptions
        );
      }
      else
      {
        _cosmosDbClient = new(
          accountEndpoint: configuration["CosmosDbEndpoint"],
          tokenCredential: credential,
          clientOptions: cosmosDbOptions
        );
      }
    }

    private async Task GetDatabaseOrCreateAsync()
    {
      DatabaseResponse response = await _cosmosDbClient.CreateDatabaseIfNotExistsAsync(id: _databaseName);
      _database = response.Database;
    }

    private async Task<Container> GetContainerOrCreateAsync(string containerName)
    {
      if (_database == null)
      {
        throw new NullReferenceException("Database cannot be null before attempting to create a container.");
      }

      ContainerResponse response = await _database.CreateContainerIfNotExistsAsync(id: containerName, partitionKeyPath: "/id", throughput: _defaultThroughput);
      return response.Container;
    }

    private async Task InitializeDatabaseAndContainersAsync()
    {
      if (!_didInitializeDb)
      {
        await GetDatabaseOrCreateAsync();
        _teamsContainer = await GetContainerOrCreateAsync(_teamsContainerName);
        _votesContainer = await GetContainerOrCreateAsync(_votesContainerName);
        _didInitializeDb = true;
      }
    }

    public async Task<IDictionary<string, IList<Team>>> GetResultsByVoter()
    {
      await InitializeDatabaseAndContainersAsync();

      if (_votesContainer == null)
      {
        throw new NullReferenceException("Votes container cannot be null");
      }

      using FeedIterator<VoterVoteResult> feed = _votesContainer.GetItemQueryIterator<VoterVoteResult>(
        queryText: $"SELECT * FROM {_votesContainerName}"
      );

      var resultDictionary = new Dictionary<string, IList<Team>>();
      while (feed.HasMoreResults)
      {
        FeedResponse<VoterVoteResult> response = await feed.ReadNextAsync();

        foreach (var result in response)
        {
          if (result.Id != null && result.TeamsVotedFor != null)
          {
            resultDictionary.Add(result.Id, result.TeamsVotedFor);
          }
        }
      }

      return resultDictionary;
    }

    public async Task<IList<Team>> GetTeams()
    {
      await InitializeDatabaseAndContainersAsync();

      if (_teamsContainer == null)
      {
        throw new NullReferenceException("Teams container cannot be null");
      }

      using FeedIterator<Team> feed = _teamsContainer.GetItemQueryIterator<Team>(
        queryText: $"SELECT * FROM {_teamsContainerName} t ORDER BY t.ordinalNumber"
      );

      var resultList = new List<Team>();
      while (feed.HasMoreResults)
      {
        FeedResponse<Team> response = await feed.ReadNextAsync();

        foreach (var result in response)
        {
          resultList.Add(result);
        }
      }

      return resultList;
    }

    public async Task<IList<TeamVoteResult>> GetVotingResults()
    {
      await InitializeDatabaseAndContainersAsync();
      var resultList = new List<TeamVoteResult>();

      var votesByVoter = await GetResultsByVoter();

      foreach (var voterId in votesByVoter.Keys)
      {
        foreach (var votedTeam in votesByVoter[voterId])
        {
          bool resultExists = false;
          foreach (var existingResult in resultList)
          {
            if (existingResult != null)
            {
              if (votedTeam.Id == existingResult.Team?.Id)
              {
                existingResult.NumVotes++;
                resultExists = true;
              }
            }
          }
          if (!resultExists)
          {
            var newResult = new TeamVoteResult { NumVotes = 1, Team = votedTeam };
            resultList.Add(newResult);
          }
        }
      }

      return resultList;

    }

    public async Task SubmitVote(string voterId, IList<Team> teamsVotedFor)
    {
      await InitializeDatabaseAndContainersAsync();

      var voterResult = new VoterVoteResult
      {
        Id = voterId,
        TeamsVotedFor = teamsVotedFor
      };

      if (_votesContainer != null)
      {
        await _votesContainer.CreateItemAsync<VoterVoteResult>(
          item: voterResult,
          partitionKey: new PartitionKey(voterId)
        );
      }

    }

    public async Task AddTeam(Team team)
    {
      await InitializeDatabaseAndContainersAsync();

      if (string.IsNullOrEmpty(team.Id))
      {
        team.Id = Guid.NewGuid().ToString();
      }

      if (_teamsContainer != null)
      {
        await _teamsContainer.CreateItemAsync<Team>(
          item: team,
          partitionKey: new PartitionKey(team.Id)
        );
      }

    }

    public Task DeleteTeam(Team team)
    {
      throw new NotImplementedException();
    }

    public Task UpdateTeam(Team team)
    {
      throw new NotImplementedException();
    }
  }
}
