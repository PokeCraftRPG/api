namespace PokeGame.Core.Rosters;

public sealed record TrainerRosters(Roster? Source, Roster? Target)
{
  public IReadOnlyCollection<Roster> Values
  {
    get
    {
      List<Roster> rosters = new(capacity: 2);
      if (Source is not null)
      {
        rosters.Add(Source);
      }
      if (Target is not null)
      {
        rosters.Add(Target);
      }
      return rosters.ToList().AsReadOnly();
    }
  }
}
