using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string OwnerId { get; private set; } = string.Empty;
  public Guid UserId { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<AbilityEntity> Abilities { get; private set; } = [];
  public List<FormEntity> Forms { get; private set; } = [];
  public List<ItemEntity> Items { get; private set; } = [];
  public List<MembershipEntity> Membership { get; private set; } = [];
  public List<MembershipInvitationEntity> MembershipInvitations { get; private set; } = [];
  public List<MoveEntity> Moves { get; private set; } = [];
  public List<RegionEntity> Regions { get; private set; } = [];
  public List<SpeciesEntity> Species { get; private set; } = [];
  public List<TrainerEntity> Trainers { get; private set; } = [];
  public List<VarietyEntity> Varieties { get; private set; } = [];
  public StorageSummaryEntity? StorageSummary { get; private set; }

  public WorldEntity(WorldCreated @event) : base(@event)
  {
    Id = new WorldId(@event.StreamId).ToGuid();

    SetOwnership(@event.OwnerId);

    Key = @event.Key.Value;
  }

  private WorldEntity() : base()
  {
  }

  public MembershipEntity? FindMembership(UserId userId) => Membership.SingleOrDefault(x => x.MemberId == userId.Value);

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    actorIds.Add(new ActorId(OwnerId));
    foreach (MembershipEntity membership in Membership)
    {
      actorIds.AddRange(membership.GetActorIds());
    }
    return actorIds;
  }

  public void GrantMembership(WorldMembershipGranted @event)
  {
    base.Update(@event);

    GrantMembership(@event.UserId, @event);
  }
  private void GrantMembership(UserId userId, DomainEvent @event)
  {
    MembershipEntity? membership = FindMembership(userId);
    if (membership is null)
    {
      membership = new MembershipEntity(this, userId, @event);
      Membership.Add(membership);
    }
    else
    {
      membership.Grant(@event);
    }
  }

  public void RevokeMembership(WorldMembershipRevoked @event)
  {
    base.Update(@event);

    MembershipEntity? membership = FindMembership(@event.UserId);
    membership?.Revoke(@event);
  }

  public void SetKey(WorldKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  private void SetOwnership(UserId ownerId)
  {
    OwnerId = ownerId.Value;
    UserId = ownerId.EntityId;
  }

  public void TransferOwnership(WorldOwnershipTransferred @event)
  {
    base.Update(@event);

    GrantMembership(new UserId(OwnerId), @event);

    SetOwnership(@event.OwnerId);
  }

  public void Update(WorldUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
