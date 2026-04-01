using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string OwnerId { get; private set; } = string.Empty;

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<AbilityEntity> Abilities { get; private set; } = [];
  public List<FormEntity> Forms { get; private set; } = [];
  public List<ItemEntity> Items { get; private set; } = [];
  public List<MemberEntity> Members { get; private set; } = [];
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

    OwnerId = @event.OwnerId.Value;

    Key = @event.Key.Value;
  }

  private WorldEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    actorIds.Add(new ActorId(OwnerId));
    foreach (MemberEntity member in Members)
    {
      actorIds.AddRange(member.GetActorIds());
    }
    return actorIds;
  }

  public void GrantMembership(WorldMembershipGranted @event)
  {
    MemberEntity? member = Members.SingleOrDefault(x => x.MemberKey == @event.UserId.Value);
    if (member is null)
    {
      member = new MemberEntity(this, @event);
      Members.Add(member);
    }
    else
    {
      member.Grant(@event);
    }
  }

  public void SetKey(WorldKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
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
