using Logitar.EventSourcing;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class PokemonSpecies : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  public new SpeciesId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Number? _number = null;
  public Number Number => _number ?? throw new InvalidOperationException("The number was not initialized.");
  public PokemonCategory Category { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The key was not initialized.");
  public Name? Name { get; private set; }
  public Description? Description { get; private set; }

  public Friendship BaseFriendship { get; private set; } = new();
  private CatchRate? _catchRate = null;
  public CatchRate CatchRate => _catchRate ?? throw new InvalidOperationException("The catch rate was not initialized.");
  public GrowthRate GrowthRate { get; private set; }

  private EggCycles? _eggCycles = null;
  public EggCycles EggCycles => _eggCycles ?? throw new InvalidOperationException("The egg cycles were not initialized.");
  public EggGroups EggGroups { get; private set; } = new();

  private readonly Dictionary<RegionId, Number> _regionalNumbers = [];
  public IReadOnlyDictionary<RegionId, Number> RegionalNumbers => _regionalNumbers.AsReadOnly();

  public PokemonSpecies() : base()
  {
  }

  public PokemonSpecies(
    World world,
    Number number,
    PokemonCategory category,
    Slug key,
    CatchRate catchRate,
    EggCycles eggCycles,
    Friendship? baseFriendship = null,
    GrowthRate growthRate = default,
    EggGroups? eggGroups = null,
    ActorId? actorId = null) : this(SpeciesId.NewId(world.Id), number, category, key, catchRate, eggCycles, baseFriendship, growthRate, eggGroups, actorId)
  {
  }

  public PokemonSpecies(
    SpeciesId speciesId,
    Number number,
    PokemonCategory category,
    Slug key,
    CatchRate catchRate,
    EggCycles eggCycles,
    Friendship? baseFriendship = null,
    GrowthRate growthRate = default,
    EggGroups? eggGroups = null,
    ActorId? actorId = null) : base(speciesId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }
    if (!Enum.IsDefined(growthRate))
    {
      throw new ArgumentOutOfRangeException(nameof(growthRate));
    }

    baseFriendship ??= new Friendship();
    eggGroups ??= new EggGroups();
    Raise(new SpeciesCreated(number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups), actorId);
  }
  protected virtual void Handle(SpeciesCreated @event)
  {
    _number = @event.Number;
    Category = @event.Category;

    _key = @event.Key;

    BaseFriendship = @event.BaseFriendship;
    _catchRate = @event.CatchRate;
    GrowthRate = @event.GrowthRate;

    _eggCycles = @event.EggCycles;
    EggGroups = @event.EggGroups;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new SpeciesDeleted(), actorId);
    }
  }

  public void Describe(Description? description, ActorId? actorId = null)
  {
    if (!Equals(Description, description))
    {
      Raise(new SpeciesDescribed(description), actorId);
    }
  }
  protected virtual void Handle(SpeciesDescribed @event)
  {
    Description = @event.Description;
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void RemoveRegionalNumber(Region region, ActorId? actorId = null) => RemoveRegionalNumber(region.Id, actorId);
  public void RemoveRegionalNumber(RegionId regionId, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, regionId);

    if (_regionalNumbers.ContainsKey(regionId))
    {
      Raise(new SpeciesRegionalNumberRemoved(regionId), actorId);
    }
  }
  protected virtual void Handle(SpeciesRegionalNumberRemoved @event)
  {
    _regionalNumbers.Remove(@event.RegionId);
  }

  public void Rename(Name? name, ActorId? actorId = null)
  {
    if (!Equals(Name, name))
    {
      Raise(new SpeciesRenamed(name), actorId);
    }
  }
  protected virtual void Handle(SpeciesRenamed @event)
  {
    Name = @event.Name;
  }

  public void SetGameData(Friendship baseFriendship, CatchRate catchRate, GrowthRate growthRate, EggCycles eggCycles, EggGroups eggGroups, ActorId? actorId = null)
  {
    if (!Equals(BaseFriendship, baseFriendship) || !Equals(CatchRate, catchRate) || !Equals(GrowthRate, growthRate)
      || !Equals(EggCycles, eggCycles) || !Equals(EggGroups, eggGroups))
    {
      Raise(new SpeciesGameDataChanged(baseFriendship, catchRate, growthRate, eggCycles, eggGroups), actorId);
    }
  }
  protected virtual void Handle(SpeciesGameDataChanged @event)
  {
    BaseFriendship = @event.BaseFriendship;
    _catchRate = @event.CatchRate;
    GrowthRate = @event.GrowthRate;

    _eggCycles = @event.EggCycles;
    EggGroups = @event.EggGroups;
  }

  public void SetKey(Slug key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new SpeciesKeyChanged(key), actorId);
    }
  }
  protected virtual void Handle(SpeciesKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetRegionalNumber(Region region, Number number, ActorId? actorId = null) => SetRegionalNumber(region.Id, number, actorId);
  public void SetRegionalNumber(RegionId regionId, Number number, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, regionId);

    if (!_regionalNumbers.TryGetValue(regionId, out Number? existingNumber) || existingNumber != number)
    {
      Raise(new SpeciesRegionalNumberChanged(regionId, number), actorId);
    }
  }
  protected virtual void Handle(SpeciesRegionalNumberChanged @event)
  {
    _regionalNumbers[@event.RegionId] = @event.Number;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
