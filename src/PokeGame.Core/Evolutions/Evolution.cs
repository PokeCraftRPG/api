using Logitar.EventSourcing;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

public sealed class Evolution : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Evolution";

  public new EvolutionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public FormId SourceId { get; private set; }
  public FormId TargetId { get; private set; }
  public EvolutionTrigger Trigger { get; private set; }

  public Level? Level { get; private set; } // TODO(fpion): >=
  public bool Friendship { get; private set; } // TODO(fpion): 170 or more.
  public Gender? Gender { get; private set; } // TODO(fpion): ==
  public ItemId? ItemId { get; private set; } // TODO(fpion): used or held.
  public MoveId? MoveId { get; private set; } // TODO(fpion): in the moveset.
  public Location? Location { get; private set; } // TODO(fpion): from the payload.
  public TimeOfDay? TimeOfDay { get; private set; } // TODO(fpion): from the payload.

  public Evolution() : base()
  {
  }

  public Evolution(World world, Form source, Form target, EvolutionTrigger trigger, Item? item = null, ActorId? actorId = null)
    : this(EvolutionId.NewId(world.Id), source, target, trigger, item, actorId)
  {
  }

  public Evolution(EvolutionId evolutionId, Form source, Form target, EvolutionTrigger trigger, Item? item = null, ActorId? actorId = null)
    : base(evolutionId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, source, nameof(source));
    WorldMismatchException.ThrowIfMismatch(this, target, nameof(target));
    if (source.Equals(target) || source.VarietyId == target.VarietyId)
    {
      // TODO(fpion): they should even be from different Pokémon species.
      throw new ArgumentException("The source and target forms must be different and from different Pokémon varieties.", nameof(target));
    }

    if (!Enum.IsDefined(trigger))
    {
      throw new ArgumentOutOfRangeException(nameof(trigger));
    }

    if (item is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, item, nameof(item));
    }
    else if (trigger == EvolutionTrigger.ItemUsed)
    {
      throw new EvolutionItemRequiredException(this);
    }

    Raise(new EvolutionCreated(source.Id, target.Id, trigger, item?.Id), actorId);
  }
  private void Handle(EvolutionCreated @event)
  {
    SourceId = @event.SourceId;
    TargetId = @event.TargetId;
    Trigger = @event.Trigger;

    ItemId = @event.ItemId;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new EvolutionDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetConditions(
    Level? level,
    bool friendship,
    Gender? gender,
    Item? item,
    Move? move,
    Location? location,
    TimeOfDay? timeOfDay,
    ActorId? actorId = null)
  {
    if (gender.HasValue && !Enum.IsDefined(gender.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(gender));
    }

    if (item is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, item, nameof(item));
    }
    else if (Trigger == EvolutionTrigger.ItemUsed)
    {
      throw new EvolutionItemRequiredException(this);
    }

    if (move is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, move, nameof(move));
    }

    if (timeOfDay.HasValue && !Enum.IsDefined(timeOfDay.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(timeOfDay));
    }

    ItemId? itemId = item?.Id;
    MoveId? moveId = move?.Id;
    if (!Equals(Level, level) || !Equals(Friendship, friendship) || !Equals(Gender, gender) || !Equals(ItemId, itemId)
      || !Equals(MoveId, moveId) || !Equals(Location, location) || !Equals(TimeOfDay, timeOfDay))
    {
      Raise(new EvolutionConditionsChanged(level, friendship, gender, itemId, moveId, location, timeOfDay), actorId);
    }
  }
  private void Handle(EvolutionConditionsChanged @event)
  {
    Level = @event.Level;
    Friendship = @event.Friendship;
    Gender = @event.Gender;
    ItemId = @event.ItemId;
    MoveId = @event.MoveId;
    Location = @event.Location;
    TimeOfDay = @event.TimeOfDay;
  }
}
