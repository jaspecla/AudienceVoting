using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{
  public class CosmosDbTeamService : ITeamService
  {

    private CosmosDbContainerService _containerService;

    private string _teamsContainerName = "Teams";
    private string _votesContainerName = "Votes";

    public CosmosDbTeamService(CosmosDbContainerService containerService)
    {
      _containerService = containerService;
    }

    public async Task<IDictionary<string, IList<Team>>> GetResultsByVoter()
    {

      var votesContainer = await _containerService.GetContainerOrCreateAsync(_votesContainerName);

      using FeedIterator<VoterVoteResult> feed = votesContainer.GetItemQueryIterator<VoterVoteResult>(
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

    public async Task<IList<TeamVoteResult>> GetVotingResults()
    {
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
      var voterResult = new VoterVoteResult
      {
        Id = voterId,
        TeamsVotedFor = teamsVotedFor
      };

      var votesContainer = await _containerService.GetContainerOrCreateAsync(_votesContainerName);

      await votesContainer.CreateItemAsync<VoterVoteResult>(
        item: voterResult,
        partitionKey: new PartitionKey(voterId)
      );

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
