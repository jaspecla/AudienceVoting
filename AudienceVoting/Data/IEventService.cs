namespace AudienceVoting.Data
{
  public interface IEventService
  {
    Task<IList<VotingEvent>> GetEvents();
    Task<VotingEvent?> GetCurrentActiveEvent();
    Task AddEvent(VotingEvent newEvent);
    Task SetActiveEvent(VotingEvent eventToActivate);
    Task DeleteEvent(VotingEvent eventToDelete);
    Task UpdateEvent(VotingEvent eventToUpdate);
  }
}
