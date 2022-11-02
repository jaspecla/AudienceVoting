using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Shared
{
  public partial class TeamBallotItem
  {
    [Parameter]
    public Team? Team { get; set; }

    [Parameter]
    public EventCallback<Team> VoteChangedCallback { get; set; }

    protected override void OnInitialized()
    {
      Team!.VotedFor = false;
    }

    private async Task OnVoteChanged()
    {

      if (Team != null)
      {
        Team.VotedFor = !Team.VotedFor;
        await VoteChangedCallback.InvokeAsync(Team);
      }

    }

  }
}
