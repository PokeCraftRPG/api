using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public class Region : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Region";

  public new RegionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Region() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
