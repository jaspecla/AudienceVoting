using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Pages
{
  public partial class Events
  {
    [Inject]
    protected IEventService? EventService { get; set; }
    protected IList<VotingEvent>? VotingEvents { get; set; }
    protected string? EventNameToAdd { get; set; }
    protected int EventNumVotesToAdd { get; set; } = 1;
    protected override async Task OnInitializedAsync()
    {
      if (EventService != null)
      {
        VotingEvents = await EventService.GetEvents();
      }
    }

    protected async Task OnActiveEventItemChangedAsync(VotingEvent newActiveEvent)
    {
      if (VotingEvents != null && EventService != null)
      {
        foreach (var eventItem in VotingEvents)
        {
          if (eventItem.Id == newActiveEvent.Id)
          {
            eventItem.IsActive = true;
          }
          else
          {
            eventItem.IsActive = false;
          }

          await EventService.UpdateEvent(eventItem);

        }
      }
    }

    protected void OnEventDeleted(VotingEvent deletedEvent)
    {
      if (VotingEvents != null)
      {
        VotingEvents.Remove(deletedEvent);
      }
    }

    protected async Task OnAddNewEventButtonClickedAsync()
    {
      if (EventService != null && !string.IsNullOrEmpty(EventNameToAdd))
      {
        var newEvent = new VotingEvent
        {
          Id = Guid.NewGuid().ToString(),
          IsActive = false,
          Name = EventNameToAdd,
          NumTeamsToVoteFor = EventNumVotesToAdd
        };

        await EventService.AddEvent(newEvent);

        if (VotingEvents != null)
        {
          VotingEvents.Add(newEvent);
        }

        EventNameToAdd = null;
        EventNumVotesToAdd = 1;
      } 
    }
  }
}
