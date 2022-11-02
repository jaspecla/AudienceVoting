using Microsoft.Azure.Cosmos;

namespace AudienceVoting.Data
{
  public class CosmosDbEventService : IEventService
  {
    private CosmosDbContainerService _containerService;

    private string _eventContainerName = "Events";

    public CosmosDbEventService(CosmosDbContainerService containerService)
    {
      _containerService = containerService;
    }


    public async Task<IList<VotingEvent>> GetEvents()
    {
      var eventContainer = await _containerService.GetContainerOrCreateAsync(_eventContainerName);

      using FeedIterator<VotingEvent> feed = eventContainer.GetItemQueryIterator<VotingEvent>(
        queryText: $"SELECT * FROM {_eventContainerName}"
      );

      var resultList = new List<VotingEvent>();
      while (feed.HasMoreResults)
      {
        FeedResponse<VotingEvent> response = await feed.ReadNextAsync();

        foreach (var result in response)
        {
          resultList.Add(result);
        }
      }

      return resultList;
    }

    private async Task<IList<VotingEvent>> GetAllEventsFlaggedActive()
    {
      var eventContainer = await _containerService.GetContainerOrCreateAsync(_eventContainerName);

      using FeedIterator<VotingEvent> feed = eventContainer.GetItemQueryIterator<VotingEvent>(
        queryText: $"SELECT * FROM {_eventContainerName} e WHERE e.isActive = true"
      );

      var resultList = new List<VotingEvent>();
      while (feed.HasMoreResults)
      {
        FeedResponse<VotingEvent> response = await feed.ReadNextAsync();

        foreach (var result in response)
        {
          resultList.Add(result);
        }
      }

      return resultList;
    }

    public async Task<VotingEvent?> GetCurrentActiveEvent()
    {
      var activeEvents = await GetAllEventsFlaggedActive();

      if (activeEvents.Count > 0)
      {
        return activeEvents[0];
      }
      else
      {
        return null;
      }
    }

    public async Task AddEvent(VotingEvent newEvent)
    {

      if (string.IsNullOrEmpty(newEvent.Id))
      {
        newEvent.Id = Guid.NewGuid().ToString();
      }

      // A new event cannot be the active event.  It must be activated.
      newEvent.IsActive = false;
      

      var eventContainer = await _containerService.GetContainerOrCreateAsync(_eventContainerName);
      await eventContainer.CreateItemAsync<VotingEvent>(
        item: newEvent,
        partitionKey: new PartitionKey(newEvent.Id)
      );

    }

    public async Task SetActiveEvent(VotingEvent eventToActivate)
    {
      if (string.IsNullOrEmpty(eventToActivate.Id))
      {
        throw new ArgumentNullException("eventToActivate.id");
      }

      // Set all active events (even if there are more than one of them, which there shouldn't be)
      // to Inactive state.
      var currentActiveEvents = await GetAllEventsFlaggedActive();

      var eventContainer = await _containerService.GetContainerOrCreateAsync(_eventContainerName);

      foreach (var e in currentActiveEvents)
      {
        // Let's not go back and forth to the database if
        // the one we want is already active.
        if (e.Id != eventToActivate.Id)
        {
          e.IsActive = false;
          await eventContainer.UpsertItemAsync<VotingEvent>(e, new PartitionKey(e.Id));
        }
      }

      eventToActivate.IsActive = true;

      await eventContainer.UpsertItemAsync<VotingEvent>(eventToActivate, new PartitionKey(eventToActivate.Id));

    }

    public async Task DeleteEvent(VotingEvent eventToDelete)
    {
      var eventContainer = await _containerService.GetContainerOrCreateAsync(_eventContainerName);

      await eventContainer.DeleteItemAsync<VotingEvent>(eventToDelete.Id, new PartitionKey(eventToDelete.Id));
    }

  }
}
