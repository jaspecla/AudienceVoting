namespace AudienceVoting.Data
{
  public interface ITeamService
  {
    Task<IList<Team>> GetTeams();
    Task SubmitVote(string voterId, IList<Team> teamsVotedFor);
    Task<IList<TeamVoteResult>> GetVotingResults();
    Task<IDictionary<string, IList<Team>>> GetResultsByVoter();
    Task AddTeam(Team team);
    Task DeleteTeam(Team team);
    Task UpdateTeam(Team team);
  }
}
