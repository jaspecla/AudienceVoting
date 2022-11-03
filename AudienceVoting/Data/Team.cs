namespace AudienceVoting.Data
{
  public class Team
  {
    public string? Id { get; set; }
    public string? EventId { get; set; }
    public string? Name { get; set; }
    public int OrdinalNumber {  get; set; }
    public bool VotedFor { get; set; }
  }
}
