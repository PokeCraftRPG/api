using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public readonly struct MembershipInvitationId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public MembershipInvitationId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, MembershipInvitation.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public MembershipInvitationId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(MembershipInvitation.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public MembershipInvitationId(string value) : this(new StreamId(value))
  {
  }

  public static MembershipInvitationId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(MembershipInvitation.EntityKind, EntityId, WorldId);

  public static bool operator ==(MembershipInvitationId left, MembershipInvitationId right) => left.Equals(right);
  public static bool operator !=(MembershipInvitationId left, MembershipInvitationId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is MembershipInvitationId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
