namespace AudienceVoting.Data
{
  public interface ITeamService
  {
    Task<IList<Team>> GetTeams();
    Task AddTeam(Team team);
    Task DeleteTeam(Team team);
    Task UpdateTeam(Team team);
  }
}
