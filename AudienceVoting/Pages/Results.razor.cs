using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Results
  {
    [Inject]
    protected IVoteService? VoteService { get; set; }

    protected IList<TeamVoteResult> VoteResults { get; set; }

    public Results()
    {
      VoteResults = new List<TeamVoteResult>();
    }

    protected override async Task OnInitializedAsync()
    {
      if (VoteService != null)
      {
        VoteResults = await VoteService.GetVotingResults();
      }
    }

  }
}
