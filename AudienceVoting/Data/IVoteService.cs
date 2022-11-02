namespace AudienceVoting.Data
{
  public interface IVoteService
  {
    Task SubmitVote(string voterId, string eventId, IList<Team> teamsVotedFor);
    Task<IList<TeamVoteResult>> GetEventVotingResults(string eventId);
    Task<IDictionary<string, IList<Team>>> GetEventResultsByVoter(string eventId);
  }
}
