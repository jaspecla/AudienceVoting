using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Shared
{
  public partial class SingleTeamItem
  {

    [Parameter]
    public Team? Team { get; set; }

    [Parameter]
    public EventCallback<Team> TeamDeletedCallback { get; set; }

    [Inject]
    protected ITeamService? TeamService { get; set; }

    private bool IsEditing = false;
    private bool IsDeleting = false;

    private async Task OnDeleteButtonClickedAsync()
    {
      if (Team != null && TeamService != null)
      {
        IsDeleting = true;
        await TeamService.DeleteTeam(Team);
        await TeamDeletedCallback.InvokeAsync(Team);
        IsDeleting = false;
      }
    }

    private void OnEditButtonClicked()
    {
      IsEditing = true;
    }

    private async Task OnSaveButtonClickedAsync()
    {
      if (Team != null && TeamService != null)
      {
        await TeamService.UpdateTeam(Team);
        IsEditing = false;
      }
    }


  }
}
