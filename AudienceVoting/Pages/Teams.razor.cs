﻿using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Teams
  {
    [Inject]
    protected ITeamService? TeamService { get; set; }

    protected List<Team> TeamList { get; set; }

    protected string? TeamNameToAdd { get; set; }

    public Teams()
    {
      TeamList = new List<Team>();
    }

    protected override async Task OnInitializedAsync()
    {
      if (TeamService != null)
      {
        // We need to convert from IList<T> to List<T> because the Reorder component
        // only accepts List<T>
        var teamsFromService = await TeamService.GetTeams();

        foreach (var team in teamsFromService)
        {
          TeamList.Add(team);
        }
      }
    }

    private async Task OnAddNewTeamButtonClickedAsync()
    {
      if (TeamService != null)
      {
        var newTeam = new Team
        {
          Id = Guid.NewGuid().ToString(),
          Name = TeamNameToAdd,
          OrdinalNumber = TeamList.Count
        };

        await TeamService.AddTeam(newTeam);

        if (TeamList != null)
        {
          TeamList.Add(newTeam);
        }

        TeamNameToAdd = null;
      }
    }

    private void OnTeamWasDeleted(Team team)
    {
      if (TeamList != null)
      {
        TeamList.Remove(team);
      }
    }

    private async Task OnTeamOrderChanged()
    {
      for (int i = 0; i < TeamList.Count; i++)
      {
        var team = TeamList[i];
        team.OrdinalNumber = i;

        if (TeamService != null)
        {
          await TeamService.UpdateTeam(team);
        }
      }
    }
  }

}

