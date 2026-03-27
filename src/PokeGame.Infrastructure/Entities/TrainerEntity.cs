using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Infrastructure.Entities;

internal class TrainerEntity : AggregateEntity
{
  public int TrainerId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string? OwnerKey { get; private set; }
  public Guid? OwnerId { get; private set; }

  public string License { get; private set; } = string.Empty;

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public TrainerGender Gender { get; private set; }
  public int Money { get; private set; }
  public int PartySize { get; private set; } // TODO(fpion): is this a computed?

  public string? Sprite { get; private set; }
  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public TrainerEntity(WorldEntity world, TrainerCreated @event) : base(@event)
  {
    Id = new TrainerId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    License = @event.License.Value;

    Key = @event.Key.Value;

    Gender = @event.Gender;
  }

  private TrainerEntity() : base()
  {
  }

  public void SetKey(TrainerKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void SetOwner(TrainerOwnershipChanged @event)
  {
    base.Update(@event);

    if (@event.OwnerId.HasValue)
    {
      OwnerKey = @event.OwnerId.Value.Value;
      OwnerId = Guid.Empty; // TODO(fpion): implement
    }
    else
    {
      OwnerKey = null;
      OwnerId = null;
    }
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
