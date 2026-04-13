using Logitar.EventSourcing;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Infrastructure.Entities;

internal class TrainerEntity : AggregateEntity
{
  public int TrainerId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string? OwnerId { get; private set; }
  public Guid? UserId { get; private set; }

  public string License { get; private set; } = string.Empty;

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public TrainerGender Gender { get; private set; }
  public int Money { get; private set; }
  public int PartySize { get; private set; }

  public string? Sprite { get; private set; }
  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public List<InventoryEntity> Inventory { get; private set; } = [];

  public TrainerEntity(WorldEntity world, TrainerCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    Id = new TrainerId(@event.StreamId).EntityId;

    License = @event.License.Value;

    Key = @event.Key.Value;

    Gender = @event.Gender;
  }

  private TrainerEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (OwnerId is not null)
    {
      actorIds.Add(new ActorId(OwnerId));
    }
    return actorIds;
  }

  public void SetKey(TrainerKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void SetOwnership(TrainerOwnershipChanged @event)
  {
    base.Update(@event);

    OwnerId = @event.OwnerId?.Value;
    UserId = @event.OwnerId?.EntityId;
  }

  public void Update(TrainerUpdated @event)
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

    if (@event.Gender.HasValue)
    {
      Gender = @event.Gender.Value;
    }
    if (@event.Money is not null)
    {
      Money = @event.Money.Value;
    }

    if (@event.Sprite is not null)
    {
      Sprite = @event.Sprite.Value?.Value;
    }
    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
