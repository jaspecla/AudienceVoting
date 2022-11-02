using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{
  public class CosmosDbVoteService : IVoteService
  {

    private CosmosDbContainerService _containerService;

    private string _votesContainerName = "Votes";

    public CosmosDbVoteService(CosmosDbContainerService containerService)
    {
      _containerService = containerService;
    }

    public async Task<IDictionary<string, IList<Team>>> GetEventResultsByVoter(string eventId)
    {

      var votesContainer = await _containerService.GetContainerOrCreateAsync(_votesContainerName);

      var query = new QueryDefinition($"SELECT * FROM {_votesContainerName} v WHERE v.eventId = @eventId")
        .WithParameter("@eventId", eventId);

      var resultDictionary = new Dictionary<string, IList<Team>>();
      using (FeedIterator<VoterVoteResult> feed = votesContainer.GetItemQueryIterator<VoterVoteResult>(query))
      {
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
      }

      return resultDictionary;
    }

    public async Task<IList<TeamVoteResult>> GetEventVotingResults(string eventId)
    {
      var resultList = new List<TeamVoteResult>();

      var votesByVoter = await GetEventResultsByVoter(eventId);

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

    public async Task SubmitVote(string voterId, string eventId, IList<Team> teamsVotedFor)
    {
      var voterResult = new VoterVoteResult
      {
        Id = voterId,
        EventId = eventId,
        TeamsVotedFor = teamsVotedFor
      };

      var votesContainer = await _containerService.GetContainerOrCreateAsync(_votesContainerName);

      await votesContainer.CreateItemAsync<VoterVoteResult>(
        item: voterResult,
        partitionKey: new PartitionKey(voterId)
      );

    }
  }
}
