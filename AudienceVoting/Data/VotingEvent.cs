namespace AudienceVoting.Data
{
  public class VotingEvent
  {
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public int NumTeamsToVoteFor;

  }
}
