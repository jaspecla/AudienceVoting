using AudienceVoting.Data;
using Microsoft.AspNetCore.Components;

namespace AudienceVoting.Shared
{
  public partial class SingleEventItem
  {
    [Parameter]
    public VotingEvent? EventItem { get; set; }
    [Parameter]
    public EventCallback<VotingEvent> ActiveEventChangedCallback { get; set; }
    [Parameter]
    public EventCallback<VotingEvent> EventItemDeletedCallback { get; set; }

    [Inject]
    protected IEventService? EventService { get; set; }

    protected bool IsDeleting = false;
    protected bool IsEditing = false;

    protected async Task OnMakeActiveButtonClickedAsync()
    {
      if (EventItem != null)
      {
        EventItem.IsActive = true;
        await ActiveEventChangedCallback.InvokeAsync(EventItem);
      }
    }

    protected void OnEditButtonClicked()
    {
      IsEditing = true;
    }

    protected async Task OnDeleteButtonClickedAsync()
    {
      if (EventService != null && EventItem != null)
      {
        IsDeleting = true;
        await EventService.DeleteEvent(EventItem);
        await EventItemDeletedCallback.InvokeAsync(EventItem);
        IsDeleting = false;
      }

    }

    protected async Task OnSaveButtonClickedAsync()
    {
      if (EventService != null && EventItem != null)
      {
        await EventService.UpdateEvent(EventItem);
        IsEditing = false;
      }
    }

  }
}
