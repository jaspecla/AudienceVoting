using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Results
  {
    [Inject]
    protected IVoteService? VoteService { get; set; }

    protected IList<TeamVoteResult> VoteResults { get; set; }
    protected string? SelectedEventId { get; set; }

    public Results()
    {
      VoteResults = new List<TeamVoteResult>();
    }

    protected async Task OnSelectedEventChangedAsync(string newEventId)
    {
      SelectedEventId = newEventId;
      if (VoteService != null && SelectedEventId != null)
      {
        VoteResults = await VoteService.GetEventVotingResults(newEventId);
      }
    }

  }
}
