namespace AudienceVoting.Data
{
  public class InMemoryTeamService : ITeamService
  {
    private IList<Team> _teams;
    private IDictionary<string, IList<Team>> _teamVotes;
    private IDictionary<string, TeamVoteResult> _results;
    public InMemoryTeamService()
    {
      _teams = new List<Team>();
      _teamVotes = new Dictionary<string, IList<Team>>();
      _results = new Dictionary<string, TeamVoteResult>();

      var team1 = new Team
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Team One",
        OrdinalNumber = 1
      };

      var team2 = new Team
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Team Two",
        OrdinalNumber = 2
      };

      var team3 = new Team
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Team Three",
        OrdinalNumber = 3
      };

      _teams.Add(team1);
      _teams.Add(team2);
      _teams.Add(team3);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<IList<Team>> GetTeams()
    {
      return _teams;
    }

    public async Task SubmitVote(string voterId, IList<Team> teamsVotedFor)
    {
      _teamVotes.Add(voterId, teamsVotedFor);
      foreach (var team in teamsVotedFor)
      {
        if (team != null && team.Id != null)
        {
          if (_results.ContainsKey(team.Id))
          {
            _results[team.Id].NumVotes++;
          }
          else
          {
            _results[team.Id] = new TeamVoteResult { Team = team, NumVotes = 1 };
          }
        }
      }
    }

    public async Task<IList<TeamVoteResult>> GetVotingResults()
    {
      return _results.Values.OrderByDescending(result => result.NumVotes).ToList();
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

  }
}
