namespace AudienceVoting.Data
{
  public interface IVoteService
  {
    Task SubmitVote(string voterId, IList<Team> teamsVotedFor);
    Task<IList<TeamVoteResult>> GetVotingResults();
    Task<IDictionary<string, IList<Team>>> GetResultsByVoter();
  }
}
