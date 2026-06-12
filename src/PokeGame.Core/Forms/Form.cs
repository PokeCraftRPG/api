using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public class Form : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Form";

  public new FormId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Form() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
