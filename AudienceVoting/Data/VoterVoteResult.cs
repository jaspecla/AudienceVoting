namespace AudienceVoting.Data
{
  public class VoterVoteResult
  {
    public string? Id { get; set; }
    public IList<Team>? TeamsVotedFor { get; set; }
  }
}
