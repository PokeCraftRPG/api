using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Core.Pokemon;

namespace PokeGame.Infrastructure.Entities;

internal class EvolutionEntity : AggregateEntity
{
  public int EvolutionId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public FormEntity? Source { get; private set; }
  public int SourceId { get; private set; }
  public FormEntity? Target { get; private set; }
  public int TargetId { get; private set; }

  public EvolutionTrigger Trigger { get; private set; }
  public ItemEntity? Item { get; private set; }
  public int? ItemId { get; private set; }

  public int? Level { get; private set; }
  public bool Friendship { get; private set; }
  public PokemonGender? Gender { get; private set; }
  public ItemEntity? HeldItem { get; private set; }
  public int? HeldItemId { get; private set; }
  public MoveEntity? KnownMove { get; private set; }
  public int? KnownMoveId { get; private set; }
  public string? Location { get; private set; }
  public TimeOfDay? TimeOfDay { get; private set; }

  public EvolutionEntity(WorldEntity world, FormEntity source, FormEntity target, ItemEntity? item, EvolutionCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    Id = new EvolutionId(@event.StreamId).EntityId;

    Source = source;
    SourceId = source.FormId;

    Target = target;
    TargetId = target.FormId;

    Trigger = @event.Trigger;
    Item = item;
    ItemId = item?.ItemId;
  }

  private EvolutionEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Source is not null)
    {
      actorIds.AddRange(Source.GetActorIds());
    }
    if (Target is not null)
    {
      actorIds.AddRange(Target.GetActorIds());
    }
    if (Item is not null)
    {
      actorIds.AddRange(Item.GetActorIds());
    }
    if (HeldItem is not null)
    {
      actorIds.AddRange(HeldItem.GetActorIds());
    }
    if (KnownMove is not null)
    {
      actorIds.AddRange(KnownMove.GetActorIds());
    }
    return actorIds;
  }

  public void Update(ItemEntity? heldItem, MoveEntity? knownMove, EvolutionUpdated @event)
  {
    base.Update(@event);

    if (@event.Level is not null)
    {
      Level = @event.Level.Value?.Value;
    }
    if (@event.Friendship.HasValue)
    {
      Friendship = @event.Friendship.Value;
    }
    if (@event.Gender is not null)
    {
      Gender = @event.Gender.Value;
    }
    if (@event.HeldItemId is not null)
    {
      HeldItem = heldItem;
      HeldItemId = heldItem?.ItemId;
    }
    if (@event.KnownMoveId is not null)
    {
      KnownMove = knownMove;
      KnownMoveId = knownMove?.MoveId;
    }
    if (@event.Location is not null)
    {
      Location = @event.Location.Value?.Value;
    }
    if (@event.TimeOfDay is not null)
    {
      TimeOfDay = @event.TimeOfDay.Value;
    }
  }
}
