using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class FullResults
  {
    [Inject]
    protected IVoteService? VoteService { get; set; }

    protected IDictionary<string, IList<Team>>? ResultsByVoter { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (VoteService != null)
      {
        ResultsByVoter = await VoteService.GetResultsByVoter();
      }
    }

  }
}
