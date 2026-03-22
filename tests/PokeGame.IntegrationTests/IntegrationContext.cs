using Krakenar.Contracts.Actors;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;

namespace PokeGame;

public record IntegrationContext : IContext
{
  public Actor Actor { get; set; } = new();
  public World? World { get; set; }

  public UserId UserId
  {
    get
    {
      if (Actor.Type != ActorType.User)
      {
        throw new NotSupportedException();
      }
      return new UserId(Actor.GetActorId());
    }
  }
  public WorldId WorldId => World?.Id ?? throw new NotSupportedException();
  public Guid WorldUid => WorldId.ToGuid();
}
