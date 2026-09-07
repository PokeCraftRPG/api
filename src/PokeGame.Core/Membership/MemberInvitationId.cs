using Logitar.EventSourcing;

namespace PokeGame.Core.Membership;

public readonly struct MemberInvitationId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public Guid EntityId { get; }

  public MemberInvitationId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, MemberInvitation.EntityKind);
    EntityId = entity.Id;
  }

  public MemberInvitationId(string value) : this(new StreamId(value))
  {
  }

  public MemberInvitationId(Guid entityId)
  {
    Entity entity = new(MemberInvitation.EntityKind, entityId);
    StreamId = new StreamId(entity.ToString());

    EntityId = entityId;
  }

  public static MemberInvitationId NewId() => new(Guid.NewGuid());

  public Entity GetEntity() => new(MemberInvitation.EntityKind, EntityId);

  public static bool operator ==(MemberInvitationId left, MemberInvitationId right) => left.Equals(right);
  public static bool operator !=(MemberInvitationId left, MemberInvitationId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is MemberInvitationId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
