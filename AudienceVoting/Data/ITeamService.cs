namespace AudienceVoting.Data
{
  public interface ITeamService
  {
    Task<IList<Team>> GetTeams();
    Task SubmitVote(string voterId, IList<Team> teamsVotedFor);
    Task<IList<TeamVoteResult>> GetVotingResults();
  }
}
