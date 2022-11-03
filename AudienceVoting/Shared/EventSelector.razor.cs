using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Shared
{
  public partial class EventSelector
  {
    [Inject]
    IEventService? EventService { get; set; }
    [Parameter]
    public EventCallback<string> EventChangedCallback { get; set; }

    protected IList<VotingEvent>? AllEvents { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (EventService != null)
      {
        AllEvents = await EventService.GetEvents();
      }

      if (AllEvents != null && AllEvents.Count > 0)
      {
        VotingEvent? activeEvent = null;
        foreach (var votingEvent in AllEvents)
        {
          if (votingEvent.IsActive)
          {
            activeEvent = votingEvent;
          }
        }

        if (activeEvent == null)
        {
          activeEvent = AllEvents[0];
        }
        await EventChangedCallback.InvokeAsync(activeEvent.Id);

      }

    }

    protected async Task OnSelectedVotingEventChangedAsync(ChangeEventArgs itemSelectedEvent)
    {
      if (itemSelectedEvent.Value != null)
      {
        await EventChangedCallback.InvokeAsync(itemSelectedEvent.Value.ToString());
      }
    }

  }
}
