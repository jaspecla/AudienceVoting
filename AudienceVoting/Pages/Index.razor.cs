using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Index
  {

    [Inject]
    protected ITeamService? TeamService { get; set; }
    protected IList<Team>? Teams { get; set; }
    protected IList<Team> TeamsVotedFor { get; set; }
    protected bool DidVote = false;

    private int NumVotesRequired = 2;
    private string VoterId { get; set; }

    public Index()
    {
      TeamsVotedFor = new List<Team>();
      VoterId = Guid.NewGuid().ToString();
    }
    protected override async Task OnInitializedAsync()
    {
      Teams = await TeamService!.GetTeams();
    }

    private void VoteChanged(Team team)
    {
      if (team.VotedFor)
      {
        if (!TeamsVotedFor.Contains(team))
        {
          TeamsVotedFor.Add(team);
        }
      }

      if (!team.VotedFor)
      {
        if (TeamsVotedFor.Contains(team))
        {
          TeamsVotedFor.Remove(team);
        }
      }
    }

    private async Task SubmitVoteAsync()
    {
      if (!DidVote)
      {
        await TeamService!.SubmitVote(VoterId, TeamsVotedFor);
        TeamsVotedFor = new List<Team>();
        DidVote = true;
      }
    }
  }
}
