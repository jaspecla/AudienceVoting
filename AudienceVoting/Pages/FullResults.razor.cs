using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class FullResults
  {
    [Inject]
    protected IVoteService? VoteService { get; set; }
    [Inject]
    protected IEventService? EventService { get; set; }

    protected IDictionary<string, IList<Team>>? ResultsByVoter { get; set; }
    protected string? SelectedEventId;

    protected override async Task OnInitializedAsync()
    {
      if (VoteService != null && EventService != null && SelectedEventId != null)
      {
        ResultsByVoter = await VoteService.GetEventResultsByVoter(SelectedEventId);
      }
    }

    protected async Task OnSelectedEventChangedAsync(string changedEventId)
    {
      SelectedEventId = changedEventId;
      if (VoteService != null)
      {
        ResultsByVoter = await VoteService.GetEventResultsByVoter(SelectedEventId);
      }
    }

  }
}
