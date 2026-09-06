using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public readonly struct MemberInvitationId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public MemberInvitationId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value);
    WorldId = entity.WorldId ?? throw new ArgumentException("A world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public MemberInvitationId(string value) : this(new StreamId(value))
  {
  }

  public MemberInvitationId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(MemberInvitation.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public static MemberInvitationId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(MemberInvitation.EntityKind, EntityId, WorldId);

  public static bool operator ==(MemberInvitationId left, MemberInvitationId right) => left.Equals(right);
  public static bool operator !=(MemberInvitationId left, MemberInvitationId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is MemberInvitationId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
