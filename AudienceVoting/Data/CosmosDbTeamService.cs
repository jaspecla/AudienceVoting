using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{
  public class CosmosDbTeamService : ITeamService
  {

    private CosmosDbContainerService _containerService;

    private string _teamsContainerName = "Teams";

    public CosmosDbTeamService(CosmosDbContainerService containerService)
    {
      _containerService = containerService;
    }


    public async Task<IList<Team>> GetTeams()
    {
      var teamsContainer = await _containerService.GetContainerOrCreateAsync(_teamsContainerName);

      using FeedIterator<Team> feed = teamsContainer.GetItemQueryIterator<Team>(
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


    public async Task AddTeam(Team team)
    {

      if (string.IsNullOrEmpty(team.Id))
      {
        team.Id = Guid.NewGuid().ToString();
      }

      var teamsContainer = await _containerService.GetContainerOrCreateAsync(_teamsContainerName);
      await teamsContainer.CreateItemAsync<Team>(
        item: team,
        partitionKey: new PartitionKey(team.Id)
      );

    }

    public async Task DeleteTeam(Team team)
    {
      var teamsContainer = await _containerService.GetContainerOrCreateAsync(_teamsContainerName);
      await teamsContainer.DeleteItemAsync<Team>(team.Id, new PartitionKey(team.Id));
    }

    public async Task UpdateTeam(Team team)
    {
      var teamsContainer = await _containerService.GetContainerOrCreateAsync(_teamsContainerName);
      await teamsContainer.UpsertItemAsync<Team>(team, new PartitionKey(team.Id));
    }
  }
}
