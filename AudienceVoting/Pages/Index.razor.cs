using AudienceVoting.Data;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Index
  {

    [Inject]
    protected ITeamService? TeamService { get; set; }
    [Inject]
    protected IVoteService? VoteService { get; set; }
    [Inject]
    protected ILocalStorageService? LocalStorageService { get; set; }

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (LocalStorageService != null)
      {
        var previousVote = await LocalStorageService.GetItemAsync<string>("vote");
        
        // This person has voted before
        if (!string.IsNullOrEmpty(previousVote))
        {
          DidVote = true;
          StateHasChanged();
        }
      }
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
        await VoteService!.SubmitVote(VoterId, TeamsVotedFor);
        TeamsVotedFor = new List<Team>();
        DidVote = true;

        if (LocalStorageService != null)
        {
          await LocalStorageService.SetItemAsync<string>("vote", VoterId);
        }

      }
    }
  }
}
