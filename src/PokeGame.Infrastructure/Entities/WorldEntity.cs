namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  private WorldEntity() : base()
  {
  }
}
